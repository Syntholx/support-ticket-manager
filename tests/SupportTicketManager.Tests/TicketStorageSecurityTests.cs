using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

public class TicketStorageSecurityTests
{
    [Theory]
    [InlineData(201, 10)]
    [InlineData(10, 5001)]
    public void Model_RejectsOversizeText(int titleLength, int descriptionLength) =>
        Assert.Throws<ArgumentException>(() => new Ticket(0, new string('a', titleLength), new string('b', descriptionLength), 2, TicketStatus.Open));

    [Fact]
    public async Task ConcurrentContexts_RejectLostUpdate()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var firstScope = factory.Services.CreateScope();
            using var secondScope = factory.Services.CreateScope();
            var first = firstScope.ServiceProvider.GetRequiredService<TicketDbContext>();
            var second = secondScope.ServiceProvider.GetRequiredService<TicketDbContext>();
            var ticket = new Ticket(0, "Concurrency", "Test", 2, TicketStatus.Open);
            first.Tickets.Add(ticket);
            await first.SaveChangesAsync();
            var stale = await second.Tickets.SingleAsync(item => item.Id == ticket.Id);
            Assert.True(ticket.TryClose());
            await first.SaveChangesAsync();
            Assert.True(stale.TryStartProgress());
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
            using var verifyScope = factory.Services.CreateScope();
            var stored = await verifyScope.ServiceProvider.GetRequiredService<TicketDbContext>().Tickets.SingleAsync();
            Assert.Equal(TicketStatus.Closed, stored.Status);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task Pagination_IsBoundedAndStable_AndRejectsInvalidParameters()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();
            string owner = await TicketTestAuthentication.RegisterAndLoginAsync(client);
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                db.Tickets.AddRange(Enumerable.Range(0, 55).Select(index => new Ticket(0, $"Ticket {index}", "Text", 2, TicketStatus.Open, owner)));
                await db.SaveChangesAsync();
            }
            var first = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/tickets");
            var second = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/tickets?page=2&pageSize=50");
            Assert.Equal(50, first.GetArrayLength());
            Assert.Equal(5, second.GetArrayLength());
            Assert.True(first[49].GetProperty("id").GetInt32() < second[0].GetProperty("id").GetInt32());
            foreach (string query in new[] { "page=0", "page=-1", "page=10001", "pageSize=101", "pageSize=0" })
            {
                using var bad = await client.GetAsync("/api/tickets?" + query);
                Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
            }
            using var large = await client.PostAsJsonAsync("/api/tickets", new { title = new string('a', 201), description = "Text" });
            Assert.Equal(HttpStatusCode.BadRequest, large.StatusCode);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }
}
