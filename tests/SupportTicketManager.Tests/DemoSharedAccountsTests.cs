using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-authored. The inherited context factory REPLACES InitialCatalog with a GUID test name.
// Its Initialize/Delete methods both verify the exact generated name before SQL writes.
public class DemoSharedAccountsTests
{
    [Fact]
    public async Task Demo_StopsCreatingTicketsAtFiveHundred()
    {
        using var factory = new DemoFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<TicketDbContext>().Database.GetDbConnection();
            Assert.StartsWith("SupportTicketManagerTests_", connection.Database);
            Assert.NotEqual(DemoMode.DatabaseName, connection.Database);
        }
        try
        {
            await factory.InitializeDatabaseAsync();
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await DemoDatabase.CreateAccountAsync(users, DemoMode.UserEmail, DemoMode.UserPassword);
                Assert.False((await users.FindByIdAsync(user.Id))!.LockoutEnabled);
                var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                db.Tickets.AddRange(Enumerable.Range(1, 499).Select(index => new Ticket(0, $"Capacity test {index}", "Isolated test", 2, TicketStatus.Open, user.Id)));
                await db.SaveChangesAsync();
            }
            using var client = factory.CreateRawClient();
            await Csrf(client);
            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email = DemoMode.UserEmail, password = DemoMode.UserPassword });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            await Csrf(client);
            using var lastAllowed = await client.PostAsJsonAsync("/api/tickets", new { title = "Last allowed ticket", description = "Isolated capacity test" });
            Assert.Equal(HttpStatusCode.Created, lastAllowed.StatusCode);
            using var rejected = await client.PostAsJsonAsync("/api/tickets", new { title = "Over limit", description = "Must not be stored" });
            Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
            using var verify = factory.Services.CreateScope();
            Assert.Equal(500, await verify.ServiceProvider.GetRequiredService<TicketDbContext>().Tickets.CountAsync());
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task SharedAccount_WrongPasswordsDoNotLockIt_LogoutPreservesOtherSession()
    {
        using var factory = new DemoFactory();
        // Extra read-only check BEFORE the inherited migration safety check.
        using (var scope = factory.Services.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<TicketDbContext>().Database.GetDbConnection();
            Assert.StartsWith("SupportTicketManagerTests_", connection.Database);
            Assert.NotEqual(DemoMode.DatabaseName, connection.Database);
            Assert.Contains(connection.DataSource, new[] { "127.0.0.1,1433", "localhost,1433" });
        }
        try
        {
            await factory.InitializeDatabaseAsync();
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await DemoDatabase.CreateAccountAsync(users, DemoMode.UserEmail, DemoMode.UserPassword);
                Assert.False((await users.FindByIdAsync(user.Id))!.LockoutEnabled);
            }
            using var first = factory.CreateRawClient();
            await Csrf(first);
            for (int attempt = 0; attempt < 6; attempt++)
            {
                using var wrong = await first.PostAsJsonAsync("/api/auth/login", new { email = DemoMode.UserEmail, password = "Wrong-Password!2026" });
                Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
            }
            using var login = await first.PostAsJsonAsync("/api/auth/login", new { email = DemoMode.UserEmail, password = DemoMode.UserPassword });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            using var second = factory.CreateRawClient();
            await Csrf(second);
            using var otherLogin = await second.PostAsJsonAsync("/api/auth/login", new { email = DemoMode.UserEmail, password = DemoMode.UserPassword });
            Assert.Equal(HttpStatusCode.OK, otherLogin.StatusCode);
            await Csrf(first);
            using var logout = await first.PostAsJsonAsync("/api/auth/logout", new { });
            Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
            using var firstMe = await first.GetAsync("/api/auth/me");
            using var secondMe = await second.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, firstMe.StatusCode);
            Assert.Equal(HttpStatusCode.OK, secondMe.StatusCode);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    private static async Task Csrf(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Remove(ApiSecurity.CsrfHeader);
        client.DefaultRequestHeaders.Add(ApiSecurity.CsrfHeader, token.GetProperty("token").GetString());
    }

    private sealed class DemoFactory : TicketDatabaseFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                // This config value satisfies the app's demo guard. It is NOT used as the
                // actual database: TicketDatabaseFactory sets its own GUID InitialCatalog
                // inside AddDbContext, then verifies it before any migration/deletion.
                var connection = new SqlConnectionStringBuilder(config.Build().GetConnectionString("TicketDatabase"));
                connection.InitialCatalog = DemoMode.DatabaseName;
                config.AddInMemoryCollection(new Dictionary<string, string?> {
                    ["Demo:Enabled"] = "true",
                    ["ConnectionStrings:TicketDatabase"] = connection.ConnectionString,
                });
            });
        }
    }
}
