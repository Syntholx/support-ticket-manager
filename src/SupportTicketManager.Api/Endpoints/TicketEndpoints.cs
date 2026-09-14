using System.Security.Claims;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(WebApplication app)
    {
        app.MapGet("/api/tickets", async (TicketDatabaseService ticketService, ClaimsPrincipal user) =>
        {
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }
            bool isSupport = user.IsInRole("Support");
            List<Ticket> tickets = await ticketService.GetActiveTicketsAsync(ownerId, isSupport);
            return Results.Ok(tickets);
        }
        ).RequireAuthorization();
        app.MapGet("/api/tickets/archived", async (TicketDatabaseService ticketService, ClaimsPrincipal user) =>
        {
            bool isSupport = user.IsInRole("Support");
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }
            List<Ticket> tickets = await ticketService.GetArchivedTicketsAsync(ownerId, isSupport);
            return Results.Ok(tickets);
        }).RequireAuthorization();

        app.MapGet("/api/tickets/{id:int}", async (int id, TicketDatabaseService ticketService, ClaimsPrincipal user) =>
        {
            bool isSupport = user.IsInRole("Support");
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }

            Ticket? foundTicket = await ticketService.GetTicketByIdAsync(id);
            if (foundTicket == null || (!isSupport && foundTicket.OwnerId != ownerId))
            {
                return Results.NotFound(new
                {
                    message = $"Nie znaleziono zgłoszenia o ID: {id}"
                });
            }

            return Results.Ok(foundTicket);

        }).RequireAuthorization();
        app.MapPost("/api/tickets", async (CreateTicketRequest request, TicketDatabaseService ticketService, ClaimsPrincipal user) =>

        {
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new
                {
                    message = "Tytuł nie może być pusty"
                });
            }
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return Results.BadRequest(new
                {
                    message = "Opis nie może być pusty"
                });
            }




            Ticket createdTicket = await ticketService.CreateTicketAsync(request.Title, request.Description, ownerId);

            return Results.Created(
                $"/api/tickets/{createdTicket.Id}", createdTicket);
        }).RequireAuthorization();
        app.MapPost("/api/tickets/{id:int}/close", async (int id, TicketDatabaseService ticketService, ClaimsPrincipal user) =>
        {
            bool isSupport = user.IsInRole("Support");

            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }

            TicketOperationResult result = await ticketService.CloseTicketAsync(id, ownerId, isSupport);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Nie znaleziono zgłoszenia"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Zgłoszenie jest już zamknięte"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization();
        app.MapPost("/api/tickets/{id:int}/reopen", async (int id, TicketDatabaseService ticketService) =>
        {
            TicketOperationResult result = await ticketService.ReopenTicketAsync(id);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Nie znaleziono zgłoszenia"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Zgłoszenie nie jest zamknięte"
                });

            }

            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

        app.MapPost("/api/tickets/{id:int}/start", async (int id, TicketDatabaseService ticketService) =>
        {
            TicketOperationResult result = await ticketService.StartTicketAsync(id);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Nie znaleziono zgłoszenia"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Nie można rozpocząć obsługi zgłoszenia"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

        app.MapPost("/api/tickets/{id:int}/priority", async (int id, TicketDatabaseService ticketService, ChangeTicketPriorityRequest request) =>
        {
            TicketOperationResult result = await ticketService.ChangePriorityAsync(id, request.Priority);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Nie znaleziono zgłoszenia"
                });
            }
            if (result.Status == TicketOperationStatus.InvalidPriority)
            {
                return Results.BadRequest(new
                {
                    message = "Priorytet musi być od 1 do 5"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

    }
}