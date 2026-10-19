using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-authored integration test. Uses only the factory's isolated local test database.
public class TicketPriorityConcurrencyTests
{
    [Fact]
    public async Task Priority_ConcurrentWrite_Returns409AndPreservesWinningValue()
    {
        using var factory = new PriorityConflictFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = TicketTestAuthentication.CreateClient(factory);
            string owner = await TicketTestAuthentication.RegisterSupportAndLoginAsync(factory, client);
            int id;
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                var ticket = new Ticket(0, "Priority conflict", "Isolated test", 2, TicketStatus.Open, owner);
                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
                id = ticket.Id;
            }

            // Commit another writer after the endpoint loaded the ticket but before its UPDATE.
            factory.Conflict.Arm(id);
            using var response = await client.PostAsJsonAsync($"/api/tickets/{id}/priority", new { priority = 4 });
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.True(factory.Conflict.Injected);

            var refreshed = await client.GetFromJsonAsync<JsonElement>($"/api/tickets/{id}");
            Assert.Equal(5, refreshed.GetProperty("priority").GetInt32());
            Assert.Equal("Open", refreshed.GetProperty("status").GetString());
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    private sealed class PriorityConflictFactory : TicketDatabaseFactory
    {
        public ConcurrentPriorityWriter Conflict { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
                services.AddDbContext<TicketDbContext>(options => options.AddInterceptors(Conflict)));
        }
    }

    private sealed class ConcurrentPriorityWriter : SaveChangesInterceptor
    {
        private int targetId;
        public bool Injected { get; private set; }
        public void Arm(int id) => targetId = id;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not TicketDbContext current || targetId == 0)
                return result;
            var changed = current.ChangeTracker.Entries<Ticket>()
                .SingleOrDefault(entry => entry.Entity.Id == targetId && entry.State == EntityState.Modified);
            if (changed is null) return result;

            int id = Interlocked.Exchange(ref targetId, 0);
            if (id == 0) return result;
            // Same isolated database; no interceptor on the competing context.
            var options = new DbContextOptionsBuilder<TicketDbContext>()
                .UseSqlServer(current.Database.GetConnectionString()).Options;
            await using var competing = new TicketDbContext(options);
            var ticket = await competing.Tickets.SingleAsync(item => item.Id == id, cancellationToken);
            Assert.True(ticket.TryChangePriority(5));
            await competing.SaveChangesAsync(cancellationToken);
            Injected = true;
            return result;
        }
    }
}
