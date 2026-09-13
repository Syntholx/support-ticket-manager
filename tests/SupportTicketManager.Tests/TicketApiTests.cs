using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

public class TicketApiTests
{
    [Fact]
    public async Task CreateTicket_ValidData_ReturnsCreated()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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
            JsonElement createdJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            int createdId = createdJson.GetProperty("id").GetInt32();
            Assert.True(createdId > 0);
            Assert.DoesNotContain(createdId, new[] { seed.OpenId, seed.ClosedId, seed.InProgressId });
            Assert.Equal($"/api/tickets/{createdId}", response.Headers.Location.ToString());
            using HttpResponseMessage getResponse = await client.GetAsync(
        response.Headers.Location);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            string? title = ticketJson.GetProperty("title").GetString();
            string? description = ticketJson.GetProperty("description").GetString();
            string? status = ticketJson.GetProperty("status").GetString();
            int id = ticketJson.GetProperty("id").GetInt32();
            Assert.Equal(createdId, id);
            Assert.Equal("Problem z logowaniem", title);
            Assert.Equal("Nie mogę wejść do panelu", description);
            Assert.Equal("Open", status);
            Assert.Equal(3, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_InvalidPriority_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_EmptyTitle_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_WhitespaceDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_PriorityOne_ReturnsCreated()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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
            JsonElement createdJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            int createdId = createdJson.GetProperty("id").GetInt32();
            Assert.True(createdId > 0);
            Assert.DoesNotContain(createdId, new[] { seed.OpenId, seed.ClosedId, seed.InProgressId });
            Assert.Equal($"/api/tickets/{createdId}", response.Headers.Location.ToString());
            using HttpResponseMessage getResponse = await client.GetAsync(
                response.Headers.Location
            );
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_PriorityFive_ReturnsCreated()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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
            JsonElement createdJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            int createdId = createdJson.GetProperty("id").GetInt32();
            Assert.True(createdId > 0);
            Assert.DoesNotContain(createdId, new[] { seed.OpenId, seed.ClosedId, seed.InProgressId });
            Assert.Equal($"/api/tickets/{createdId}", response.Headers.Location.ToString());
            using HttpResponseMessage getResponse = await client.GetAsync(
               response.Headers.Location
           );
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CloseTicket_InProgressTicket_ReturnsOk()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.InProgressId}/close", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync(
         $"/api/tickets/{seed.InProgressId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

            string? status = ticketJson.GetProperty("status").GetString();

            Assert.Equal("Closed", status);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CloseTicket_MissingTicket_ReturnsNotFound()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                "/api/tickets/99/close", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Nie znaleziono zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CloseTicket_AlreadyClosedTicket_ReturnsConflict()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.ClosedId}/close", null);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Zgłoszenie jest już zamknięte", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ReopenTicket_ClosedTicket_ReturnsOkAndChangesStatus()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.ClosedId}/reopen", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync(
     $"/api/tickets/{seed.ClosedId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

            string? status = ticketJson.GetProperty("status").GetString();

            Assert.Equal("Open", status);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ReopenTicket_OpenTicket_ReturnsConflict()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.OpenId}/reopen", null);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Zgłoszenie nie jest zamknięte", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ReopenTicket_InProgressTicket_ReturnsConflict()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.InProgressId}/reopen", null);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);


            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            string? message = ticketJson.GetProperty("message").GetString();

            Assert.Equal("Zgłoszenie nie jest zamknięte", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ReopenTicket_MissingTicket_ReturnsNotFound()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                "/api/tickets/99/reopen", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            string? message = ticketJson.GetProperty("message").GetString();

            Assert.Equal("Nie znaleziono zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task OpenTicket_StartProgress_ReturnsOKAndChangeStatus()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.OpenId}/start", null
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync(
     $"/api/tickets/{seed.OpenId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

            string? status = ticketJson.GetProperty("status").GetString();

            Assert.Equal("InProgress", status);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task InProgressTicket_StartProgress_ReturnConflictAndMessage()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.InProgressId}/start", null
            );
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Nie można rozpocząć obsługi zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ClosedTicket_StartProgress_ReturnConflictAndMessage()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                 $"/api/tickets/{seed.ClosedId}/start", null
            );
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Nie można rozpocząć obsługi zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task NoFoundTicket_StartProgress_ReturnNoFoundAndMessage()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsync(
                "/api/tickets/99/start", null
            );
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Nie znaleziono zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_ValidValue_UpdatesTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 4 }
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync(
     $"/api/tickets/{seed.OpenId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

            int priority = ticketJson.GetProperty("priority").GetInt32();

            Assert.Equal(4, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_InvalidValue_KeepsPreviousPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 8 }
            );
            JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = errorJson.GetProperty("message").GetString();
            Assert.Equal("Priorytet musi być od 1 do 5", message);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.OpenId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(3, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_MissingTicket_ReturnsNotFound()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                "/api/tickets/99/priority", new { priority = 4 }
            );
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            JsonElement ticketJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = ticketJson.GetProperty("message").GetString();
            Assert.Equal("Nie znaleziono zgłoszenia", message);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_Zero_ReturnsBadRequestAndKeepsPreviousPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 0 });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = errorJson.GetProperty("message").GetString();
            Assert.Equal("Priorytet musi być od 1 do 5", message);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.OpenId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(3, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_Six_ReturnsBadRequestAndKeepsPreviousPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 6 });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            JsonElement errorJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            string? message = errorJson.GetProperty("message").GetString();
            Assert.Equal("Priorytet musi być od 1 do 5", message);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.OpenId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(3, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_One_ReturnsOkAndUpdatesPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 1 });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.OpenId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(1, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_Five_ReturnsOkAndUpdatesPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.OpenId}/priority", new { priority = 5 });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.OpenId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(5, priority);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ChangePriority_ClosedTicket_ReturnsOkAndUpdatesPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                 $"/api/tickets/{seed.ClosedId}/priority", new { priority = 2 });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using HttpResponseMessage getResponse = await client.GetAsync($"/api/tickets/{seed.ClosedId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            JsonElement ticketJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
            int priority = ticketJson.GetProperty("priority").GetInt32();
            Assert.Equal(2, priority);
            string? status = ticketJson.GetProperty("status").GetString();
            Assert.Equal("Closed", status);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_PriorityZero_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_PrioritySix_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_MissingDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_NullDescription_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_NonNumericPriority_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
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

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task CreateTicket_InvalidJson_ReturnsBadRequestAndDoesNotCreateTicket()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();
        try
        {
            await factory.InitializeDatabaseAsync();
            var seed = await SeedTicketsAsync(factory);
            using HttpClient client = factory.CreateClient();
            using StringContent content = new StringContent(
        "{\"title\":",
        System.Text.Encoding.UTF8,
        "application/json");

            using HttpResponseMessage response =
                await client.PostAsync("/api/tickets", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await AssertSeedUnchangedAsync(client, seed);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    // Test-only data: each test receives its own database and IDs assigned by SQL.
    private static async Task<(int OpenId, int ClosedId, int InProgressId)> SeedTicketsAsync(
        TicketDatabaseFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        TicketDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TicketDbContext>();

        Ticket open = new Ticket(0, "Problem z logowaniem",
            "Użytkownik nie może wejść do panelu", 3, TicketStatus.Open);
        Ticket closed = new Ticket(0, "Błąd płatności",
            "Płatność wymaga sprawdzenia", 5, TicketStatus.Closed);
        Ticket inProgress = new Ticket(0, "Problem z wysyłką",
            "Nie można nadać paczki", 4, TicketStatus.InProgress);

        dbContext.Tickets.Add(open);
        dbContext.Tickets.Add(closed);
        dbContext.Tickets.Add(inProgress);
        await dbContext.SaveChangesAsync();

        return (open.Id, closed.Id, inProgress.Id);
    }

    private static async Task AssertSeedUnchangedAsync(
        HttpClient client, (int OpenId, int ClosedId, int InProgressId) seed)
    {
        using HttpResponseMessage activeResponse = await client.GetAsync("/api/tickets");
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        JsonElement active = await activeResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, active.ValueKind);
        Assert.Equal(2, active.GetArrayLength());
        Assert.Equal(seed.InProgressId, active[0].GetProperty("id").GetInt32());
        Assert.Equal(4, active[0].GetProperty("priority").GetInt32());
        Assert.Equal("InProgress", active[0].GetProperty("status").GetString());
        Assert.Equal(seed.OpenId, active[1].GetProperty("id").GetInt32());
        Assert.Equal(3, active[1].GetProperty("priority").GetInt32());
        Assert.Equal("Open", active[1].GetProperty("status").GetString());

        using HttpResponseMessage archivedResponse = await client.GetAsync("/api/tickets/archived");
        Assert.Equal(HttpStatusCode.OK, archivedResponse.StatusCode);
        JsonElement archived = await archivedResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, archived.ValueKind);
        Assert.Equal(1, archived.GetArrayLength());
        Assert.Equal(seed.ClosedId, archived[0].GetProperty("id").GetInt32());
        Assert.Equal(5, archived[0].GetProperty("priority").GetInt32());
        Assert.Equal("Closed", archived[0].GetProperty("status").GetString());
    }
}





