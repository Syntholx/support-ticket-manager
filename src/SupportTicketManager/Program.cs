List<Ticket> tickets = CreateSampleTickets();


Console.WriteLine($"Liczba zgłoszeń: {tickets.Count}");
DisplayTickets(tickets);

List<Ticket> urgentTickets = GetUrgentTickets(tickets);

Console.WriteLine($"Liczba pilnych zgłoszeń: {urgentTickets.Count}");

bool hasCriticalTicket = HasCriticalTicket(tickets);

Console.WriteLine($"Czy istnieje zgłoszenie krytyczne: {hasCriticalTicket}");



int openTicketCount = CountOpenTickets(tickets);

Console.WriteLine($"Liczba otwartych zgłoszeń: {openTicketCount}");


List<Ticket> ticketsByPriority = SortTicketsByPriority(tickets);
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
    .Where(ticket => ticket.IsUrgent())
.ToList();
    return filteredTickets;
}

static bool HasCriticalTicket(List<Ticket> ticketsToCheck)
{
    bool result = ticketsToCheck
    .Any(ticket => ticket.IsCritical());
    return result;
}

static int CountOpenTickets(List<Ticket> ticketsToCheck)
{
    int openTicketCount = ticketsToCheck.Count(ticket => ticket.IsOpen());
    return openTicketCount;
}


static List<Ticket> SortTicketsByPriority(List<Ticket> ticketsToSort)
{
    List<Ticket> ticketsByPriority = ticketsToSort.OrderByDescending(ticket => ticket.Priority)
   .ToList();
    return ticketsByPriority;
}

static List<Ticket> CreateSampleTickets()
{
    Ticket firstTicket = new Ticket(

 id: 1,
 title: "Problem z logowaniem",
 description: "Użytkownik nie może zalogować się do panelu ",
 priority: 4,
  status: "Open"
);


    Ticket secondTicket = new Ticket(

        2,
        "Błąd płatności",
        "Płatność została pobrana, ale zamówienie nie powstało",
        5,
        "Open"
    );

    Ticket thirdTicket = new Ticket(

        3,
        "Pytanie o fakturę",
        "Użytkownik prosi o kopię faktury",
        2,
        "InProgress"
    );
    Ticket fourthTicket = new Ticket(
        4,
        "Reset hasła zakończony",
       "Użytkownik odzyskał dostęp do konta",
        3,
        "Closed"
    );


    List<Ticket> sampleTickets = new List<Ticket>
    {
    firstTicket,
    secondTicket,
    thirdTicket,
    fourthTicket
    };
    return sampleTickets;
}