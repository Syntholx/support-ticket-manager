using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-assisted integration tests; only isolated databases, no fake authentication.
public class TicketOwnershipTests
{
    [Fact]
    public async Task CreateTicket_Anonymous_ReturnsUnauthorizedAndLeavesDatabaseEmpty()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/tickets",
                new { title = "Test", description = "Bez sesji" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(response.Headers.Location);
            using IServiceScope scope = factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            Assert.Empty(await database.Tickets.AsNoTracking().ToListAsync());
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task CreateTicket_Authenticated_StoresLoggedInAccountAsOwner()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            string ownerId = await TicketTestAuthentication.RegisterAndLoginAsync(client);
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/tickets",
                new { title = "Moje zgłoszenie", description = "Zalogowany autor" });

            await AssertCreatedAndStoredAsync(factory, client, response, ownerId);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task CreateTicket_WithAnotherAccountsOwnerId_IgnoresItAndStoresLoggedInOwner()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient author = TicketTestAuthentication.CreateClient(factory);
            string authorId = await TicketTestAuthentication.RegisterAndLoginAsync(author, "author@example.com");
            using HttpClient other = TicketTestAuthentication.CreateClient(factory);
            string otherId = await TicketTestAuthentication.RegisterAndLoginAsync(other, "other@example.com");
            Assert.NotEqual(authorId, otherId);

            // Use an existing second account: the FK alone must not be what prevents impersonation.
            using HttpResponseMessage response = await author.PostAsJsonAsync("/api/tickets",
                new { title = "Próba podszycia", description = "Cudze ID w JSON", ownerId = otherId });

            await AssertCreatedAndStoredAsync(factory, author, response, authorId);
            using IServiceScope scope = factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            Assert.False(await database.Tickets.AnyAsync(ticket => ticket.OwnerId == otherId));
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    private static async Task AssertCreatedAndStoredAsync(TicketDatabaseFactory factory,
        HttpClient client, HttpResponseMessage response, string expectedOwnerId)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonElement created = await response.Content.ReadFromJsonAsync<JsonElement>();
        int id = created.GetProperty("id").GetInt32();
        Assert.True(id > 0);
        Assert.Equal(expectedOwnerId, created.GetProperty("ownerId").GetString());
        Assert.Equal(2, created.GetProperty("priority").GetInt32());
        Assert.Equal("Open", created.GetProperty("status").GetString());
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/tickets/{id}", response.Headers.Location.ToString());

        using HttpResponseMessage get = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        JsonElement read = await get.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(id, read.GetProperty("id").GetInt32());
        Assert.Equal(expectedOwnerId, read.GetProperty("ownerId").GetString());
        Assert.Equal(2, read.GetProperty("priority").GetInt32());
        Assert.Equal("Open", read.GetProperty("status").GetString());

        // Fresh context verifies SQL storage, not just the response or tracked object.
        using IServiceScope scope = factory.Services.CreateScope();
        TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        Ticket stored = Assert.Single(await database.Tickets.AsNoTracking().ToListAsync());
        Assert.Equal(id, stored.Id);
        Assert.Equal(expectedOwnerId, stored.OwnerId);
        Assert.Equal(2, stored.Priority);
        Assert.Equal(TicketStatus.Open, stored.Status);
        Assert.True(await database.Users.AnyAsync(account => account.Id == expectedOwnerId));
    }
}
