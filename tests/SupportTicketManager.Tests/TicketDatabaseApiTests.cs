using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SupportTicketManager.Tests;

public class TicketDatabaseApiTests
{
    [Fact]
    public async Task GetTickets_EmptyDatabase_ReturnsOkAndEmptyList()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            await TicketTestAuthentication.RegisterAndLoginAsync(client);

            using HttpResponseMessage response =
                await client.GetAsync("/api/tickets");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement tickets =
                await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(JsonValueKind.Array, tickets.ValueKind);
            Assert.Equal(0, tickets.GetArrayLength());
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
    [Fact]
    public async Task CreateTicket_WithoutPriority_ReturnsCreatedAndStoresDefaultPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            await TicketTestAuthentication.RegisterAndLoginAsync(client);
            using HttpResponseMessage response = await client.PostAsJsonAsync("api/tickets", new
            {
                title = "Test zapisu",
                description = "Zgłoszenie zapisane w SQL"
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            JsonElement createdTicket = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(2, createdTicket.GetProperty("priority").GetInt32());
            Assert.Equal("Open", createdTicket.GetProperty("status").GetString());
            Assert.NotNull(response.Headers.Location);
            using HttpResponseMessage getResponse = await client.GetAsync(response.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticket = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            string? title = ticket.GetProperty("title").GetString();
            string? description = ticket.GetProperty("description").GetString();
            string? status = ticket.GetProperty("status").GetString();
            int priority = ticket.GetProperty("priority").GetInt32();
            Assert.Equal("Test zapisu", title);
            Assert.Equal("Zgłoszenie zapisane w SQL", description);
            Assert.Equal(2, priority);
            Assert.Equal(createdTicket.GetProperty("id").GetInt32(), ticket.GetProperty("id").GetInt32());
            Assert.Equal("Open", status);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
    [Fact]
    public async Task CreateTicket_WithClientPriority_ReturnsCreatedAndStoresDefaultPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            await TicketTestAuthentication.RegisterAndLoginAsync(client);
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/tickets", new
            {
                title = "Test priorytetu",
                description = "Próba wymuszenia priorytetu przez klienta",
                priority = 5
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            JsonElement createdTicket = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(2, createdTicket.GetProperty("priority").GetInt32());
            Assert.Equal("Open", createdTicket.GetProperty("status").GetString());
            Assert.NotNull(response.Headers.Location);
            using HttpResponseMessage getResponse = await client.GetAsync(response.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticket = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(2, ticket.GetProperty("priority").GetInt32());
            Assert.Equal("Open", ticket.GetProperty("status").GetString());
            Assert.Equal(createdTicket.GetProperty("id").GetInt32(), ticket.GetProperty("id").GetInt32());
            Assert.Equal("Test priorytetu", ticket.GetProperty("title").GetString());
            Assert.Equal("Próba wymuszenia priorytetu przez klienta", ticket.GetProperty("description").GetString());
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
    [Fact]
    public async Task GetTicketById_MissingTicket_ReturnsNotFound()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            await TicketTestAuthentication.RegisterAndLoginAsync(client);

            using HttpResponseMessage response =
                await client.GetAsync("/api/tickets/99");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            JsonElement ticket = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticket.GetProperty("message").GetString();
            Assert.Equal("Nie znaleziono zgłoszenia o ID: 99", message);

        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

}
