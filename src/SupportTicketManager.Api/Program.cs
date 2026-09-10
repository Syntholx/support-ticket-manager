using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
var app = builder.Build();
List<Ticket> tickets = new List<Ticket>

{
    new Ticket(
        1,
        "Problem z logowaniem",
        "Użytkownik nie może wejść do panelu",
        3,
        TicketStatus.Open),

        new Ticket(
            2,
            "Błąd płatności",
            "Płatność wymaga sprawdzenia",
            5,
            TicketStatus.Closed),

            new Ticket(
                3,
                "Problem z wysyłką",
                "Nie można nadać paczki",
                4,
                TicketStatus.InProgress)
};
TicketQueries ticketQueries = new TicketQueries();
TicketService ticketService = new TicketService(tickets);
app.MapGet("api/status", () => new
{
    name = "Support Ticket Manager",
    isRunning = true,
    version = "0.7.0"
});
app.MapGet("api/name", () => "Support Ticket Manager");
app.MapGet("/api/tickets", () =>
{
    List<Ticket> activeTickets = ticketQueries.GetActiveTickets(tickets);
    List<Ticket> sortedTickets = ticketQueries.SortTicketsByPriority(activeTickets);
    return sortedTickets;
}
);
app.MapGet("/api/tickets/archived", () => ticketQueries.GetClosedTickets(tickets));
app.MapGet("/api/tickets/{id:int}", (int id) =>
{
    Ticket? foundTicket = ticketQueries.FindTicketById(tickets, id);
    if (foundTicket == null)
    {
        return Results.NotFound(new
        {
            message = $"Nie znaleziono zgłoszenia o ID: {id}"
        });
    }
    return Results.Ok(foundTicket);
});
app.MapPost("/api/tickets", (CreateTicketRequest request) =>

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

    Ticket createdTicket = ticketService.CreateTicket(request.Title, request.Description, request.Priority);

    return Results.Created(
        $"/api/tickets/{createdTicket.Id}", createdTicket);
});
app.MapPost("/api/tickets/{id:int}/close", (int id) =>
{
    Ticket? foundTicket = ticketQueries.FindTicketById(tickets, id);
    if (foundTicket == null)
    {
        return Results.NotFound(new
        {
            message = "Nie znaleziono zgłoszenia"
        });
    }
    bool wasClose = foundTicket.TryClose();
    if (wasClose == false)
    {
        return Results.Conflict(new
        {
            message = "Zgłoszenie jest już zamknięte"
        });
    }
    return Results.Ok(foundTicket);
});
app.MapPost("/api/tickets/{id:int}/reopen", (int id) =>
{
    Ticket? foundTicket = ticketQueries.FindTicketById(tickets, id);
    if (foundTicket == null)
    {
        return Results.NotFound(new
        {
            message = "Nie znaleziono zgłoszenia"
        });
    }
    bool wasReopen = foundTicket.TryReopen();
    if (wasReopen == false)
    {
        return Results.Conflict(new
        {
            message = "Zgłoszenie nie jest zamknięte"
        });

    }

    return Results.Ok(foundTicket);
});

app.MapPost("/api/tickets/{id:int}/start", (int id) =>
{
    Ticket? foundTicket = ticketQueries.FindTicketById(tickets, id);
    if (foundTicket == null)
    {
        return Results.NotFound(new
        {
            message = "Nie znaleziono zgłoszenia"
        });
    }
    bool wasTryStartProgress = foundTicket.TryStartProgress();
    if (wasTryStartProgress == false)
    {
        return Results.Conflict(new
        {
            message = "Nie można rozpocząć obsługi zgłoszenia"
        });
    }
    return Results.Ok(foundTicket);
});

app.MapPost("/api/tickets/{id:int}/priority", (int id, ChangeTicketPriorityRequest request) =>
{
    Ticket? foundTicket = ticketQueries.FindTicketById(tickets, id);
    if (foundTicket == null)
    {
        return Results.NotFound(new
        {
            message = "Nie znaleziono zgłoszenia"
        });
    }
    bool wasChangePriority = foundTicket.TryChangePriority(request.Priority);
    if (wasChangePriority == false)
    {
        return Results.BadRequest(new
        {
            message = "Priorytet musi być od 1 do 5"
        });
    }
    return Results.Ok(foundTicket);
});

app.Run();
public partial class Program
{

}
