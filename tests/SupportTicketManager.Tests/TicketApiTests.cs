using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
namespace SupportTicketManager.Tests;

public class TicketApiTests
{
    [Fact]
    public async Task CreateTicket_ValidData_ReturnsCreated()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        description = "Nie mogę wejść do panelu",
        priority = 3
    });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/api/tickets/4", response.Headers.Location.ToString());
        using HttpResponseMessage getResponse = await client.GetAsync(
    response.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    }
    [Fact]
    public async Task CreateTicket_InvalidPriority_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Problem z logowaniem",
                description = "Nie mogę wejść do panelu",
                priority = 8
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
          "/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

    }
    [Fact]
    public async Task CreateTicket_EmptyTitle_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "",
                description = "Nie mogę wejść do panelu",
                priority = 3
            });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
            "/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

    }
    [Fact]
    public async Task CreateTicket_WhitespaceDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Problem z logowaniem",
                description = "   ",
                priority = 3
            }
        );
        using HttpResponseMessage getResponse = await client.GetAsync(
            "/api/tickets/4"
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_PriorityOne_ReturnsCreated()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Problem z logowaniem",
                description = "Nie mogę wejść do panelu",
                priority = 1
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/api/tickets/4", response.Headers.Location.ToString());
        using HttpResponseMessage getResponse = await client.GetAsync(
            response.Headers.Location
        );
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_PriorityFive_ReturnsCreated()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Problem z logowaniem",
                description = "Nie mogę wejść do panelu",
                priority = 5
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/api/tickets/4", response.Headers.Location.ToString());
        using HttpResponseMessage getResponse = await client.GetAsync(
           response.Headers.Location
       );
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
    [Fact]
    public async Task CloseTicket_InProgressTicket_ReturnsOk()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/3/close", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
    "/api/tickets/3");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        string? status = ticketJson.GetProperty("status").GetString();

        Assert.Equal("Closed", status);
    }
    [Fact]
    public async Task CloseTicket_MissingTicket_ReturnsNotFound()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/99/close", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Nie znaleziono zgłoszenia", message);
    }
    [Fact]
    public async Task CloseTicket_AlreadyClosedTicket_ReturnsConflict()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/2/close", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Zgłoszenie jest już zamknięte", message);

    }
    [Fact]
    public async Task ReopenTicket_ClosedTicket_ReturnsOkAndChangesStatus()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/2/reopen", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
"/api/tickets/2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        string? status = ticketJson.GetProperty("status").GetString();

        Assert.Equal("Open", status);
    }
    [Fact]
    public async Task ReopenTicket_OpenTicket_ReturnsConflict()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/1/reopen", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Zgłoszenie nie jest zamknięte", message);
    }
    [Fact]
    public async Task ReopenTicket_InProgressTicket_ReturnsConflict()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/3/reopen", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);


        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        string? message = ticketJson.GetProperty("message").GetString();

        Assert.Equal("Zgłoszenie nie jest zamknięte", message);
    }
    [Fact]
    public async Task ReopenTicket_MissingTicket_ReturnsNotFound()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/99/reopen", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        string? message = ticketJson.GetProperty("message").GetString();

        Assert.Equal("Nie znaleziono zgłoszenia", message);
    }
}





