using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-assisted test helper: real registration and cookie login on the isolated database.
internal static class TicketTestAuthentication
{
    public static async Task<string> RegisterSupportAndLoginAsync(
        TicketDatabaseFactory factory, HttpClient client,
        string email = "test-support@example.com")
    {
        string id = await RegisterAndLoginAsync(client, email);
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            await SupportRoleSetup.AssignAsync(email,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
        }
        // Refresh the cookie after granting the role; never forge a role claim.
        using HttpResponseMessage login = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Tsm-Testowe!2026" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return id;
    }

    public static HttpClient CreateClient(TicketDatabaseFactory factory) => factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client, string email = "ticket-author@example.com")
    {
        const string password = "Tsm-Testowe!2026";
        using HttpResponseMessage registration = await client.PostAsJsonAsync(
            "/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        using HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using HttpResponseMessage me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        JsonElement identity = await me.Content.ReadFromJsonAsync<JsonElement>();
        string? id = identity.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));
        return id!;
    }
}
