using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

// AI-assisted access-control matrix. Every case uses real cookie authentication and its own SQL database.
public class TicketAuthorizationTests
{
    [Theory]
    [InlineData("GET", "/api/tickets")]
    [InlineData("GET", "/api/tickets/archived")]
    [InlineData("GET", "/api/tickets/{id}")]
    [InlineData("POST", "/api/tickets")]
    [InlineData("POST", "/api/tickets/{id}/close")]
    [InlineData("POST", "/api/tickets/{id}/start")]
    [InlineData("POST", "/api/tickets/{id}/reopen")]
    [InlineData("POST", "/api/tickets/{id}/priority")]
    public async Task TicketEndpoint_Anonymous_Returns401WithoutChangingDatabase(string method, string path)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        path = path.Replace("{id}", sandbox.Tickets["AOpen"].Id.ToString());
        using HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "POST")
            request.Content = JsonContent.Create(new { title = "Test", description = "Opis", priority = 5 });
        using HttpResponseMessage response = await sandbox.Anonymous.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Equal(sandbox.Initial, await sandbox.ReadStatesAsync());
    }

    [Theory]
    [InlineData("A", false)]
    [InlineData("B", false)]
    [InlineData("Support", false)]
    [InlineData("A", true)]
    [InlineData("B", true)]
    [InlineData("Support", true)]
    public async Task Lists_ReturnOnlyAuthorizedTicketsWithCorrectStatuses(string actor, bool archive)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        using HttpResponseMessage response = await sandbox.Client(actor).GetAsync(
            archive ? "/api/tickets/archived" : "/api/tickets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        TicketState[] expected = sandbox.Initial.Where(ticket =>
            (archive ? ticket.Status == TicketStatus.Closed : ticket.Status != TicketStatus.Closed)
            && (actor == "Support" || ticket.OwnerId == sandbox.Owner(actor))).ToArray();
        int[] actualIds = body.EnumerateArray().Select(ticket => ticket.GetProperty("id").GetInt32()).ToArray();
        Assert.Equal(expected.Select(ticket => ticket.Id).OrderBy(id => id), actualIds.OrderBy(id => id));
        if (!archive)
            Assert.Equal(expected.Select(ticket => ticket.Priority).OrderByDescending(p => p),
                body.EnumerateArray().Select(ticket => ticket.GetProperty("priority").GetInt32()));
        foreach (JsonElement ticket in body.EnumerateArray())
        {
            TicketState stored = Assert.Single(expected, item => item.Id == ticket.GetProperty("id").GetInt32());
            Assert.Equal(stored.OwnerId, ticket.GetProperty("ownerId").GetString());
            Assert.Equal(stored.Status.ToString(), ticket.GetProperty("status").GetString());
        }
        Assert.Equal(sandbox.Initial, await sandbox.ReadStatesAsync());
    }

    [Theory]
    [InlineData("/api/tickets")]
    [InlineData("/api/tickets/archived")]
    public async Task Lists_AccountWithoutTickets_ReturnOkAndEmptyArray(string path)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        using HttpClient emptyAccount = TicketTestAuthentication.CreateClient(sandbox.Factory);
        await TicketTestAuthentication.RegisterAndLoginAsync(emptyAccount, "empty@example.com");
        using HttpResponseMessage response = await emptyAccount.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        Assert.Equal(0, body.GetArrayLength());
        Assert.Equal(sandbox.Initial, await sandbox.ReadStatesAsync());
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("Support")]
    public async Task Details_EnforcesOwnerOrSupportAndHidesUnauthorizedData(string actor)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        foreach (TicketState ticket in sandbox.Initial)
        {
            using HttpResponseMessage response = await sandbox.Client(actor).GetAsync($"/api/tickets/{ticket.Id}");
            bool allowed = actor == "Support" || ticket.OwnerId == sandbox.Owner(actor);
            Assert.Equal(allowed ? HttpStatusCode.OK : HttpStatusCode.NotFound, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (allowed)
            {
                Assert.Equal(ticket.Id, body.GetProperty("id").GetInt32());
                Assert.Equal(ticket.OwnerId, body.GetProperty("ownerId").GetString());
                Assert.Equal(ticket.Title, body.GetProperty("title").GetString());
            }
            else
            {
                Assert.Equal("message", Assert.Single(body.EnumerateObject().ToArray()).Name);
                Assert.Equal($"Nie znaleziono zgłoszenia o ID: {ticket.Id}", body.GetProperty("message").GetString());
            }
        }
        using HttpResponseMessage missing = await sandbox.Client(actor).GetAsync("/api/tickets/2147483647");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        JsonElement missingBody = await missing.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("message", Assert.Single(missingBody.EnumerateObject().ToArray()).Name);
        Assert.Equal(sandbox.Initial, await sandbox.ReadStatesAsync());
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("Support")]
    public async Task Close_EnforcesAccessBeforeBusinessRuleAndOnlyChangesAllowedTicket(string actor)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        TicketState[] expected = sandbox.Initial.ToArray();
        foreach (TicketState ticket in sandbox.Initial)
        {
            using HttpResponseMessage response = await sandbox.Client(actor).PostAsync(
                $"/api/tickets/{ticket.Id}/close", null);
            bool allowed = actor == "Support" || ticket.OwnerId == sandbox.Owner(actor);
            HttpStatusCode status = !allowed ? HttpStatusCode.NotFound :
                ticket.Status == TicketStatus.Closed ? HttpStatusCode.Conflict : HttpStatusCode.OK;
            Assert.Equal(status, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (status == HttpStatusCode.OK)
            {
                Assert.Equal(ticket.Id, body.GetProperty("id").GetInt32());
                Assert.Equal("Closed", body.GetProperty("status").GetString());
                expected = expected.Select(item => item.Id == ticket.Id ? item with { Status = TicketStatus.Closed } : item).ToArray();
            }
            else
            {
                Assert.Equal("message", Assert.Single(body.EnumerateObject().ToArray()).Name);
            }
            Assert.Equal(expected, await sandbox.ReadStatesAsync());
        }
    }

    [Theory]
    [InlineData("A", "start")]
    [InlineData("A", "reopen")]
    [InlineData("A", "priority")]
    [InlineData("B", "start")]
    [InlineData("B", "reopen")]
    [InlineData("B", "priority")]
    [InlineData("Support", "start")]
    [InlineData("Support", "reopen")]
    [InlineData("Support", "priority")]
    public async Task SupportOperations_RejectOrdinaryAccountsEvenForOwnTickets(string actor, string operation)
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        TicketState[] expected = sandbox.Initial.ToArray();
        TicketState[] targets = actor == "Support"
            ? sandbox.Initial.Where(ticket => operation == "priority" ||
                (operation == "start" ? ticket.Status == TicketStatus.Open : ticket.Status == TicketStatus.Closed)).ToArray()
            : sandbox.Initial;
        foreach (TicketState ticket in targets)
        {
            using HttpResponseMessage response = operation == "priority"
                ? await sandbox.Client(actor).PostAsJsonAsync($"/api/tickets/{ticket.Id}/priority", new { priority = 5 })
                : await sandbox.Client(actor).PostAsync($"/api/tickets/{ticket.Id}/{operation}", null);
            Assert.Equal(actor == "Support" ? HttpStatusCode.OK : HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(response.Headers.Location);
            if (actor == "Support")
            {
                TicketState changed = operation == "priority" ? ticket with { Priority = 5 } :
                    ticket with { Status = operation == "start" ? TicketStatus.InProgress : TicketStatus.Open };
                expected = expected.Select(item => item.Id == ticket.Id ? changed : item).ToArray();
                JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
                Assert.Equal(changed.Id, body.GetProperty("id").GetInt32());
                Assert.Equal(changed.Priority, body.GetProperty("priority").GetInt32());
                Assert.Equal(changed.Status.ToString(), body.GetProperty("status").GetString());
            }
            Assert.Equal(expected, await sandbox.ReadStatesAsync());
        }
    }

    [Fact]
    public async Task SupportRoleSetup_RepeatedAssignmentDoesNotGrantOtherAccountsRoles()
    {
        await using Sandbox sandbox = await Sandbox.CreateAsync();
        using IServiceScope scope = sandbox.Factory.Services.CreateScope();
        UserManager<ApplicationUser> users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SupportRoleSetup.AssignAsync("support@example.com", users, roles);
        await SupportRoleSetup.AssignAsync("support@example.com", users, roles);
        TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        IdentityRole role = Assert.Single(await database.Roles.AsNoTracking().ToListAsync());
        Assert.Equal("Support", role.Name);
        IdentityUserRole<string> assignment = Assert.Single(await database.UserRoles.AsNoTracking().ToListAsync());
        Assert.Equal(sandbox.SupportId, assignment.UserId);
        Assert.Equal(role.Id, assignment.RoleId);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SupportRoleSetup.AssignAsync("missing@example.com", users, roles));
        Assert.Single(await database.UserRoles.AsNoTracking().ToListAsync());
    }

    private record TicketState(int Id, string Title, string Description, int Priority, TicketStatus Status, string? OwnerId);

    private sealed class Sandbox : IAsyncDisposable
    {
        public TicketDatabaseFactory Factory { get; } = new();
        public HttpClient A { get; private set; } = null!;
        public HttpClient B { get; private set; } = null!;
        public HttpClient Support { get; private set; } = null!;
        public HttpClient Anonymous { get; private set; } = null!;
        public string AId { get; private set; } = "";
        public string BId { get; private set; } = "";
        public string SupportId { get; private set; } = "";
        public Dictionary<string, Ticket> Tickets { get; } = new();
        public TicketState[] Initial { get; private set; } = [];
        public HttpClient Client(string actor) => actor switch { "A" => A, "B" => B, _ => Support };
        public string Owner(string actor) => actor switch { "A" => AId, "B" => BId, _ => SupportId };

        public static async Task<Sandbox> CreateAsync()
        {
            Sandbox sandbox = new();
            try
            {
                await sandbox.Factory.InitializeDatabaseAsync();
                sandbox.A = TicketTestAuthentication.CreateClient(sandbox.Factory);
                sandbox.B = TicketTestAuthentication.CreateClient(sandbox.Factory);
                sandbox.Support = TicketTestAuthentication.CreateClient(sandbox.Factory);
                sandbox.Anonymous = TicketTestAuthentication.CreateClient(sandbox.Factory);
                sandbox.AId = await TicketTestAuthentication.RegisterAndLoginAsync(sandbox.A, "a@example.com");
                sandbox.BId = await TicketTestAuthentication.RegisterAndLoginAsync(sandbox.B, "b@example.com");
                sandbox.SupportId = await TicketTestAuthentication.RegisterSupportAndLoginAsync(sandbox.Factory, sandbox.Support, "support@example.com");
                using IServiceScope scope = sandbox.Factory.Services.CreateScope();
                TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                foreach (var owner in new[] { ("A", (string?)sandbox.AId), ("B", (string?)sandbox.BId), ("Legacy", (string?)null) })
                {
                    foreach (TicketStatus status in new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Closed })
                    {
                        string key = owner.Item1 + status;
                        int priority = status == TicketStatus.Open ? 2 : status == TicketStatus.InProgress ? 4 : 5;
                        Ticket ticket = new(0, key, "Opis " + key, priority, status, owner.Item2);
                        sandbox.Tickets.Add(key, ticket);
                        database.Tickets.Add(ticket);
                    }
                }
                await database.SaveChangesAsync();
                sandbox.Initial = await sandbox.ReadStatesAsync();
                return sandbox;
            }
            catch
            {
                await sandbox.DisposeAsync();
                throw;
            }
        }

        public async Task<TicketState[]> ReadStatesAsync()
        {
            using IServiceScope scope = Factory.Services.CreateScope();
            TicketDbContext database = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            List<Ticket> tickets = await database.Tickets.AsNoTracking().OrderBy(ticket => ticket.Id).ToListAsync();
            return tickets.Select(ticket => new TicketState(ticket.Id, ticket.Title, ticket.Description,
                ticket.Priority, ticket.Status, ticket.OwnerId)).ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            A?.Dispose(); B?.Dispose(); Support?.Dispose(); Anonymous?.Dispose();
            try { await Factory.DeleteDatabaseAsync(); }
            finally { Factory.Dispose(); }
        }
    }
}
