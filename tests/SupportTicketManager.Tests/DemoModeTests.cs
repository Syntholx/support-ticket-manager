using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-authored. These tests never connect to SQL or modify the local application database.
public class DemoModeTests
{
    private static WebApplicationFactory<Program> Factory() => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
                ["Demo:Enabled"] = "true",
                ["ConnectionStrings:TicketDatabase"] = "Server=127.0.0.1,1433;Database=SupportTicketManagerDemo;Integrated Security=true;Encrypt=true",
            }));
        });

    [Theory]
    [InlineData("register")]
    [InlineData("confirm-email")]
    [InlineData("resend-confirmation")]
    [InlineData("forgot-password")]
    [InlineData("reset-password")]
    [InlineData("change-password")]
    public async Task Demo_BlocksAccountChangesBeforeBindingOrDatabase(string route)
    {
        using var factory = Factory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.PostAsync($"/api/auth/{route}", new StringContent("invalid JSON"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Demo_StillRequiresCsrfForLogin()
    {
        using var factory = Factory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = DemoMode.UserEmail, password = DemoMode.UserPassword });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Invalid or missing CSRF token.", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("title").GetString());
    }

    [Fact]
    public async Task Demo_DoesNotAllowOtherAccountLoginsOrSendEmail()
    {
        using var factory = Factory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Add(ApiSecurity.CsrfHeader, csrf.GetProperty("token").GetString());
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "other@example.com", password = "Not-A-Real-Password!2026" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IAccountEmail>()
            .SendAsync("other@example.com", "subject", "link", CancellationToken.None));
    }

    [Theory]
    [InlineData("SupportTicketManager")]
    [InlineData("master")]
    [InlineData("SupportTicketManagerDemoBackup")]
    public void Demo_RefusesOtherDatabases(string name)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["ConnectionStrings:TicketDatabase"] = $"Server=localhost;Database={name};Integrated Security=true"
        }).Build();
        Assert.Throws<InvalidOperationException>(() => DemoMode.ValidateDatabase(config));
    }

    [Fact]
    public void Seed_ContainsTwelveValidTicketsForBothRoles()
    {
        var tickets = DemoDatabase.CreateTickets("regular", "support");
        Assert.Equal(12, tickets.Length);
        Assert.Contains(tickets, ticket => ticket.OwnerId == "support");
        Assert.Contains(tickets, ticket => ticket.OwnerId == "regular");
        Assert.Contains(tickets, ticket => ticket.Status == TicketStatus.Closed);
        Assert.Contains(tickets, ticket => ticket.Status == TicketStatus.Open);
        Assert.Contains(tickets, ticket => ticket.Status == TicketStatus.InProgress);
        Assert.All(tickets, ticket => Assert.InRange(ticket.Priority, 1, 5));
    }
}
