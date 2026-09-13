using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// Prepared with AI assistance; every test uses a separate disposable SQL database.
public class AuthApiTests
{
    private const string TestPassword = "Tsm-Testowe!2026";

    [Fact]
    public async Task Register_ValidData_ReturnsCreatedAndStoresHashedPassword()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                email = "registration@example.com",
                password = TestPassword
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(JsonValueKind.Object, body.ValueKind);
            // Only the public message is allowed, not user/password/hash fields.
            JsonProperty property = Assert.Single(body.EnumerateObject().ToArray());
            Assert.Equal("message", property.Name);
            Assert.Equal("Konto zostało utworzone.", property.Value.GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            ApplicationUser user = Assert.Single(await database.Users.AsNoTracking().ToListAsync());
            Assert.Equal("registration@example.com", user.Email);
            Assert.Equal(user.Email, user.UserName);
            // Boolean assertions deliberately avoid printing the stored hash on failure.
            Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
            Assert.False(user.PasswordHash == TestPassword);
            IPasswordHasher<ApplicationUser> hasher =
                scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            Assert.True(hasher.VerifyHashedPassword(user, user.PasswordHash!, TestPassword)
                != PasswordVerificationResult.Failed);
            Assert.Equal(PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(user, user.PasswordHash!, "Wrong-Test-Password!"));
            Assert.Empty(await database.UserRoles.ToListAsync());
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequestAndKeepsOneAccount()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = factory.CreateClient();
            var request = new { email = "duplicate@example.com", password = TestPassword };
            using HttpResponseMessage firstResponse = await client.PostAsJsonAsync("/api/auth/register", request);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
            using HttpResponseMessage secondResponse = await client.PostAsJsonAsync("/api/auth/register", request);
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
            JsonElement body = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Nie udało się utworzyć konta", body.GetProperty("message").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            ApplicationUser user = Assert.Single(await database.Users.AsNoTracking().ToListAsync());
            Assert.Equal(request.email, user.Email);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Theory]
    [InlineData("weak-password@example.com", "abc")]
    [InlineData("to-nie-email", TestPassword)]
    public async Task Register_InvalidData_ReturnsBadRequestAndLeavesUsersEmpty(string email, string password)
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Nie udało się utworzyć konta", body.GetProperty("message").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            Assert.Empty(await database.Users.AsNoTracking().ToListAsync());
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
}
