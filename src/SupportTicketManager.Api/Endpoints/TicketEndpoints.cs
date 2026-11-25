using System.Security.Claims;
using System.Data;
using Microsoft.EntityFrameworkCore;

public static class TicketEndpoints
{
    private static bool ValidPage(int? page, int? pageSize) =>
        (page is null or >= 1 and <= 10000) && (pageSize is null or >= 1 and <= 100);
    public static void MapTicketEndpoints(WebApplication app)
    {
        app.MapGet("/api/tickets", async (TicketDatabaseService ticketService, ClaimsPrincipal user, int? page, int? pageSize, CancellationToken cancellationToken) =>
        {
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }
            bool isSupport = user.IsInRole("Support");
            if (!ValidPage(page, pageSize)) return Results.BadRequest(new { message = "page: 1–10000; pageSize: 1–100." });
            List<Ticket> tickets = await ticketService.GetActiveTicketsAsync(ownerId, isSupport, page ?? 1, pageSize ?? 50, cancellationToken);
            return Results.Ok(tickets);
        }
        ).RequireAuthorization();
        app.MapGet("/api/tickets/archived", async (TicketDatabaseService ticketService, ClaimsPrincipal user, int? page, int? pageSize, CancellationToken cancellationToken) =>
        {
            bool isSupport = user.IsInRole("Support");
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }
            if (!ValidPage(page, pageSize)) return Results.BadRequest(new { message = "page: 1–10000; pageSize: 1–100." });
            List<Ticket> tickets = await ticketService.GetArchivedTicketsAsync(ownerId, isSupport, page ?? 1, pageSize ?? 50, cancellationToken);
            return Results.Ok(tickets);
        }).RequireAuthorization();

        app.MapGet("/api/tickets/{id:int}", async (int id, TicketDatabaseService ticketService, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            bool isSupport = user.IsInRole("Support");
            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }

            Ticket? foundTicket = await ticketService.GetTicketByIdAsync(id, cancellationToken);
            if (foundTicket == null || (!isSupport && foundTicket.OwnerId != ownerId))
            {
                return Results.NotFound(new
                {
                    message = $"No ticket found with ID: {id}"
                });
            }

            return Results.Ok(foundTicket);

        }).RequireAuthorization();
        app.MapPost("/api/tickets", async (CreateTicketRequest request, TicketDatabaseService ticketService, TicketDbContext database, ClaimsPrincipal user, CancellationToken cancellationToken) =>

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
                    message = "Title cannot be empty"
                });
            }
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return Results.BadRequest(new
                {
                    message = "Description cannot be empty"
                });
            }




            if (request.Title.Length > Ticket.MaxTitleLength || request.Description.Length > Ticket.MaxDescriptionLength)
                return Results.BadRequest(new { message = "Maximum length: 200 characters for the title and 5000 for the description." });
            // Bound storage in the public demo. Serialize count+insert against other creates/resets.
            await using var capacityTransaction = DemoMode.IsEnabled(app.Configuration)
                ? await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) : null;
            if (capacityTransaction is not null)
            {
                await database.Database.ExecuteSqlRawAsync("SELECT COUNT_BIG(*) FROM dbo.Tickets WITH (TABLOCKX, HOLDLOCK)", cancellationToken);
                if (await database.Tickets.CountAsync(cancellationToken) >= 500)
                    return Results.Json(new { message = "Demo ticket limit reached. Try again after the next data reset." }, statusCode: 429);
            }
            Ticket createdTicket = await ticketService.CreateTicketAsync(request.Title, request.Description, ownerId, cancellationToken);
            if (capacityTransaction is not null) await capacityTransaction.CommitAsync(cancellationToken);

            return Results.Created(
                $"/api/tickets/{createdTicket.Id}", createdTicket);
        }).RequireAuthorization();
        app.MapPost("/api/tickets/{id:int}/close", async (int id, TicketDatabaseService ticketService, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            bool isSupport = user.IsInRole("Support");

            string? ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return Results.Unauthorized();
            }

            TicketOperationResult result = await ticketService.CloseTicketAsync(id, ownerId, isSupport, cancellationToken);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Ticket not found"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Ticket is already closed"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization();
        app.MapPost("/api/tickets/{id:int}/reopen", async (int id, TicketDatabaseService ticketService, CancellationToken cancellationToken) =>
        {
            TicketOperationResult result = await ticketService.ReopenTicketAsync(id, cancellationToken);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Ticket not found"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Ticket is not closed"
                });

            }

            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

        app.MapPost("/api/tickets/{id:int}/start", async (int id, TicketDatabaseService ticketService, CancellationToken cancellationToken) =>
        {
            TicketOperationResult result = await ticketService.StartTicketAsync(id, cancellationToken);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Ticket not found"
                });
            }

            if (result.Status == TicketOperationStatus.Conflict)
            {
                return Results.Conflict(new
                {
                    message = "Cannot start work on this ticket"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

        app.MapPost("/api/tickets/{id:int}/priority", async (int id, TicketDatabaseService ticketService, ChangeTicketPriorityRequest request, CancellationToken cancellationToken) =>
        {
            TicketOperationResult result = await ticketService.ChangePriorityAsync(id, request.Priority, cancellationToken);
            if (result.Status == TicketOperationStatus.NotFound)
            {
                return Results.NotFound(new
                {
                    message = "Ticket not found"
                });
            }
            if (result.Status == TicketOperationStatus.InvalidPriority)
            {
                return Results.BadRequest(new
                {
                    message = "Priority must be between 1 and 5"
                });
            }
            return Results.Ok(result.Ticket);
        }).RequireAuthorization("SupportOnly");

    }
}
