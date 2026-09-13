using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-assisted integration tests. Never use the application's real user database.
public class AuthSessionTests
{
    private const string Email = "session@example.com";
    private const string Password = "Tsm-Testowe!2026";

    private static HttpClient CreateClient(TicketDatabaseFactory factory) => factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    private static async Task RegisterAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = Email, password = Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task LoginAndLogout_SameClient_GainsThenLosesAccessWithoutDeletingAccount()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = CreateClient(factory);
            await RegisterAsync(client);
            using HttpResponseMessage before = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, before.StatusCode);

            using HttpResponseMessage login = await client.PostAsJsonAsync("/api/auth/login",
                new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            JsonElement loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("message", Assert.Single(loginBody.EnumerateObject().ToArray()).Name);
            Assert.Equal("Zalogowano.", loginBody.GetProperty("message").GetString());
            // Do not print authentication cookie values in test failures.
            Assert.True(login.Headers.TryGetValues("Set-Cookie", out var cookies));
            string? authCookie = cookies!.FirstOrDefault(value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
            Assert.True(authCookie is not null);
            Assert.True(authCookie!.Contains("httponly", StringComparison.OrdinalIgnoreCase));
            Assert.True(authCookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
            Assert.True(authCookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));

            using HttpResponseMessage me = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            JsonElement identity = await me.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(2, identity.EnumerateObject().Count());
            Assert.Equal(Email, identity.GetProperty("name").GetString());
            string? accountId;
            using (IServiceScope scope = factory.Services.CreateScope())
            {
                TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                ApplicationUser account = Assert.Single(await database.Users.AsNoTracking().ToListAsync());
                accountId = account.Id;
                Assert.Equal(accountId, identity.GetProperty("id").GetString());
            }

            using HttpResponseMessage logout = await client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
            JsonElement logoutBody = await logout.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Wylogowano", logoutBody.GetProperty("message").GetString());
            using HttpResponseMessage after = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
            Assert.Null(after.Headers.Location);
            using IServiceScope finalScope = factory.Services.CreateScope();
            TicketDbContext finalDatabase = finalScope.ServiceProvider.GetRequiredService<TicketDbContext>();
            ApplicationUser remaining = Assert.Single(await finalDatabase.Users.AsNoTracking().ToListAsync());
            Assert.Equal(accountId, remaining.Id);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Theory]
    [InlineData("/api/auth/me", "GET")]
    [InlineData("/api/auth/logout", "POST")]
    public async Task ProtectedEndpoint_NoCookie_Returns401WithoutRedirect(string path, string method)
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = CreateClient(factory);
            using HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path);
            using HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Theory]
    [InlineData(Email, "Wrong-Test!2026")]
    [InlineData("missing@example.com", Password)]
    public async Task Login_InvalidCredentials_Returns401AndDoesNotAuthenticate(string email, string password)
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = CreateClient(factory);
            await RegisterAsync(client);
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            using HttpResponseMessage me = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
            Assert.Null(me.Headers.Location);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Theory]
    [InlineData("", Password)]
    [InlineData(Email, " ")]
    public async Task Login_MissingData_Returns400AndDoesNotAuthenticate(string email, string password)
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = CreateClient(factory);
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            using HttpResponseMessage me = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }
}
