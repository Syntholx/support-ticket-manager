public static class TicketEndpoints
{
    public static void MapTicketEndpoints(WebApplication app)
    {
        app.MapGet("/api/tickets", async (TicketDatabaseService ticketService) =>
        {
            List<Ticket> tickets = await ticketService.GetActiveTicketsAsync();
            return Results.Ok(tickets);
        }
        );
        app.MapGet("/api/tickets/archived", async (TicketDatabaseService ticketService) =>
        {
            List<Ticket> tickets = await ticketService.GetArchivedTicketsAsync();
            return Results.Ok(tickets);
        });

        app.MapGet("/api/tickets/{id:int}", async (int id, TicketDatabaseService ticketService) =>
        {
            Ticket? foundTicket = await ticketService.GetTicketByIdAsync(id);
            if (foundTicket == null)
            {
                return Results.NotFound(new
                {
                    message = $"Nie znaleziono zgłoszenia o ID: {id}"
                });
            }
            return Results.Ok(foundTicket);
        });
        app.MapPost("/api/tickets", async (CreateTicketRequest request, TicketDatabaseService ticketService) =>

        {
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

            if (request.Priority < 1 || request.Priority > 5)
            {
                return Results.BadRequest(new
                {
                    message = "Priorytet musi być od 1 do 5"
                });
            }

            Ticket createdTicket = await ticketService.CreateTicketAsync(request.Title, request.Description, request.Priority);

            return Results.Created(
                $"/api/tickets/{createdTicket.Id}", createdTicket);
        });
        app.MapPost("/api/tickets/{id:int}/close", async (int id, TicketDatabaseService ticketService) =>
        {
            TicketOperationResult result = await ticketService.CloseTicketAsync(id);
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
        });
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
        });

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
        });

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
        });

    }
}