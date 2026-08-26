Ticket firstTicket = new Ticket
{
    Id = 1,
    Title = "Problem z logowaniem",
    Description = "Użytkownik nie może zalogować się do panelu ",
    Priority = 4,
    Status = "Open"
};


Ticket secondTicket = new Ticket
{
    Id = 2,
    Title = "Błąd płatności",
    Description = "Płatność została pobrana, ale zamówienie nie powstało",
    Priority = 5,
    Status = "Open"
};

Ticket thirdTicket = new Ticket
{
    Id = 3,
    Title = "Pytanie o fakturę",
    Description = "Użytkownik prosi o kopię faktury",
    Priority = 2,
    Status = "InProgress"
};
Ticket fourthTicket = new Ticket
{
    Id = 4,
    Title = "Reset hasła zakończony",
    Description = "Użytkownik odzyskał dostęp do konta",
    Priority = 3,
    Status = "Closed"
};


List<Ticket> tickets = new List<Ticket>
{
    firstTicket,
    secondTicket,
    thirdTicket,
    fourthTicket
};

Console.WriteLine($"Liczba zgłoszeń: {tickets.Count}");
DisplayTickets(tickets);

List<Ticket> urgentTickets = GetUrgentTickets(tickets);

Console.WriteLine($"Liczba pilnych zgłoszeń: {urgentTickets.Count}");

bool hasCriticalTicket = HasCriticalTicket(tickets);

Console.WriteLine($"Czy istnieje zgłoszenie krytyczne: {hasCriticalTicket}");

int openTicketCount = tickets.Count(ticket => ticket.Status != "Closed");

Console.WriteLine($"Liczba otwartych zgłoszeń: {openTicketCount}");

List<Ticket> ticketsByPriority = tickets
.OrderByDescending(ticket => ticket.Priority)
.ToList();
DisplayTickets(ticketsByPriority);



static void DisplayTickets(List<Ticket> ticketsToDisplay)
{
    foreach (Ticket ticket in ticketsToDisplay)
    {
        Console.WriteLine($"#{ticket.Id} | {ticket.Title} | Priority: {ticket.Priority} | {ticket.Status}");
    }
}


static List<Ticket> GetUrgentTickets(List<Ticket> ticketsToFilter)
{
    List<Ticket> filteredTickets = ticketsToFilter
    .Where(ticket => ticket.Priority >= 4)
.ToList();
    return filteredTickets;
}

static bool HasCriticalTicket(List<Ticket> ticketsToCheck)
{
    bool result = ticketsToCheck
    .Any(ticket => ticket.Priority == 5);
    return result;
}
