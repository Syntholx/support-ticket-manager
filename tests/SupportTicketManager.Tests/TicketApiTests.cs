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
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        string? title = ticketJson.GetProperty("title").GetString();
        string? description = ticketJson.GetProperty("description").GetString();
        string? status = ticketJson.GetProperty("status").GetString();
        int id = ticketJson.GetProperty("id").GetInt32();
        Assert.Equal(4, id);
        Assert.Equal("Problem z logowaniem", title);
        Assert.Equal("Nie mogę wejść do panelu", description);
        Assert.Equal("Open", status);
        Assert.Equal(3, priority);

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
    [Fact]
    public async Task OpenTicket_StartProgress_ReturnsOKAndChangeStatus()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/1/start", null
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
"/api/tickets/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        string? status = ticketJson.GetProperty("status").GetString();

        Assert.Equal("InProgress", status);
    }
    [Fact]
    public async Task InProgressTicket_StartProgress_ReturnConflictAndMessage()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/3/start", null
        );
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Nie można rozpocząć obsługi zgłoszenia", message);
    }
    [Fact]
    public async Task ClosedTicket_StartProgress_ReturnConflictAndMessage()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/2/start", null
        );
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Nie można rozpocząć obsługi zgłoszenia", message);
    }
    [Fact]
    public async Task NoFoundTicket_StartProgress_ReturnNoFoundAndMessage()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/tickets/99/start", null
        );
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Nie znaleziono zgłoszenia", message);
    }
    [Fact]
    public async Task ChangePriority_ValidValue_UpdatesTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 4 }
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
"/api/tickets/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        int priority = ticketJson.GetProperty("priority").GetInt32();

        Assert.Equal(4, priority);
    }
    [Fact]
    public async Task ChangePriority_InvalidValue_KeepsPreviousPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 8 }
        );
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Priorytet musi być od 1 do 5", message);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(3, priority);

    }
    [Fact]
    public async Task ChangePriority_MissingTicket_ReturnsNotFound()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/99/priority", new { priority = 4 }
        );
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = ticketJson.GetProperty("message").GetString();
        Assert.Equal("Nie znaleziono zgłoszenia", message);
    }
    [Fact]
    public async Task ChangePriority_Zero_ReturnsBadRequestAndKeepsPreviousPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Priorytet musi być od 1 do 5", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(3, priority);
    }
    [Fact]
    public async Task ChangePriority_Six_ReturnsBadRequestAndKeepsPreviousPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 6 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Priorytet musi być od 1 do 5", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(3, priority);
    }
    [Fact]
    public async Task ChangePriority_One_ReturnsOkAndUpdatesPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 1 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(1, priority);
    }
    [Fact]
    public async Task ChangePriority_Five_ReturnsOkAndUpdatesPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/1/priority", new { priority = 5 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(5, priority);
    }
    [Fact]
    public async Task ChangePriority_ClosedTicket_ReturnsOkAndUpdatesPriority()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/tickets/2/priority", new { priority = 2 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/2");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        int priority = ticketJson.GetProperty("priority").GetInt32();
        Assert.Equal(2, priority);
        string? status = ticketJson.GetProperty("status").GetString();
        Assert.Equal("Closed", status);
    }
    [Fact]
    public async Task CreateTicket_PriorityZero_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        description = "Nie mogę wejść do panelu",
        priority = 0
    });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Priorytet musi być od 1 do 5", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_PrioritySix_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        description = "Nie mogę wejść do panelu",
        priority = 6
    });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Priorytet musi być od 1 do 5", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_MissingDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        priority = 3
    });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Opis nie może być pusty", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

    }
    [Fact]
    public async Task CreateTicket_NullDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        description = (string?)null,
        priority = 3
    });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? message = errorJson.GetProperty("message").GetString();
        Assert.Equal("Opis nie może być pusty", message);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_NonNumericPriority_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
    "/api/tickets",
    new
    {
        title = "Problem z logowaniem",
        description = "Nie mogę się zalogować",
        priority = "abc"
    });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateTicket_InvalidJson_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using StringContent content = new StringContent(
    "{\"title\":",
    System.Text.Encoding.UTF8,
    "application/json");

        using HttpResponseMessage response =
            await client.PostAsync("/api/tickets", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync("/api/tickets/4");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}





