using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

public class AccountSecurityTests
{
    private const string Email = "security@example.com";
    private const string Password = "Security-Test!2026";

    [Fact]
    public async Task Confirmation_ExpiredToken_IsRejected_AndResendIsThrottledPerAccount()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            factory.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DataProtectionTokenProviderOptions>>()
                .Value.TokenLifespan = TimeSpan.FromSeconds(-1);
            using var client = factory.CreateClient();
            using var registration = await client.PostAsJsonAsync("/api/auth/register", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
            var data = factory.Inbox.Latest(Email);
            using var expired = await client.PostAsJsonAsync("/api/auth/confirm-email", new { userId = data["userId"], token = data["token"] });
            Assert.Equal(HttpStatusCode.BadRequest, expired.StatusCode);
            using var otherClient = factory.CreateClient();
            using var resend = await otherClient.PostAsJsonAsync("/api/auth/resend-confirmation", new { email = Email });
            Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
            Assert.Single(factory.Inbox.Messages);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task FiveWrongPasswords_LockAccount_EvenWithCorrectPassword()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();
            using var registration = await client.PostAsJsonAsync("/api/auth/register", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
            await ((TestBrowserClient)client).ConfirmEmailAsync(Email);
            for (int index = 0; index < 5; index++)
            {
                using var rejected = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = "Wrong-password!2026" });
                Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
            }
            using var correct = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.Unauthorized, correct.StatusCode);
            using var scope = factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True(await users.IsLockedOutAsync((await users.FindByEmailAsync(Email))!));
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task Confirmation_Required_BoundToUser_AndCannotBeReplayed()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();
            using var registration = await client.PostAsJsonAsync("/api/auth/register", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
            using var before = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.Unauthorized, before.StatusCode);
            var data = factory.Inbox.Latest(Email);
            using var wrongUser = await client.PostAsJsonAsync("/api/auth/confirm-email", new { userId = "missing", token = data["token"] });
            Assert.Equal(HttpStatusCode.BadRequest, wrongUser.StatusCode);
            await ((TestBrowserClient)client).ConfirmEmailAsync(Email);
            using var replay = await client.PostAsJsonAsync("/api/auth/confirm-email", new { userId = data["userId"], token = data["token"] });
            Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
            using var after = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task ResetPassword_RevokesSessionAndToken_OldPasswordStopsWorking()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var loggedIn = factory.CreateClient();
            await TicketTestAuthentication.RegisterAndLoginAsync(loggedIn, Email);
            using var recovery = factory.CreateClient();
            using var forgot = await recovery.PostAsJsonAsync("/api/auth/forgot-password", new { email = Email });
            Assert.Equal(HttpStatusCode.Accepted, forgot.StatusCode);
            var token = factory.Inbox.Latest(Email)["token"];
            var request = new { email = Email, token, password = "Replacement-Test!2026" };
            using var reset = await recovery.PostAsJsonAsync("/api/auth/reset-password", request);
            Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
            using var me = await loggedIn.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
            using var replay = await recovery.PostAsJsonAsync("/api/auth/reset-password", request);
            Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
            using var oldLogin = await recovery.PostAsJsonAsync("/api/auth/login", new { email = Email, password = "Tsm-Testowe!2026" });
            Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
            using var newLogin = await recovery.PostAsJsonAsync("/api/auth/login", new { email = Email, password = request.password });
            Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task MissingAccountAndExistingAccount_HaveSameRecoveryResponse()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();
            await TicketTestAuthentication.RegisterAndLoginAsync(client, Email);
            using var existing = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = Email });
            using var missing = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "missing@example.com" });
            Assert.Equal(HttpStatusCode.Accepted, existing.StatusCode);
            Assert.Equal(existing.StatusCode, missing.StatusCode);
            Assert.Equal(await existing.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task DeliveryFailure_LeavesUnconfirmedAccount_ResendRecovers()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();
            factory.Inbox.FailDelivery = true;
            using var registration = await client.PostAsJsonAsync("/api/auth/register", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.ServiceUnavailable, registration.StatusCode);
            factory.Inbox.FailDelivery = false;
            using var resend = await client.PostAsJsonAsync("/api/auth/resend-confirmation", new { email = Email });
            Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
            await ((TestBrowserClient)client).ConfirmEmailAsync(Email);
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }

    [Fact]
    public async Task LoggedInMutation_WithoutCsrf_DoesNotChangeTicket()
    {
        using var factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateRawClient();
            using (var scope = factory.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                Assert.True((await users.CreateAsync(new ApplicationUser { UserName = Email, Email = Email, EmailConfirmed = true }, Password)).Succeeded);
            }
            var csrf = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/auth/csrf");
            client.DefaultRequestHeaders.Add(ApiSecurity.CsrfHeader, csrf.GetProperty("token").GetString());
            using var login = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            client.DefaultRequestHeaders.Remove(ApiSecurity.CsrfHeader);
            using var rejected = await client.PostAsJsonAsync("/api/tickets", new { title = "Blocked", description = "Blocked" });
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
            using var finalScope = factory.Services.CreateScope();
            Assert.Empty(await finalScope.ServiceProvider.GetRequiredService<TicketDbContext>().Tickets.ToListAsync());
        }
        finally { await factory.DeleteDatabaseAsync(); }
    }
}
