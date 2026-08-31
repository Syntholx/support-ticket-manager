List<Ticket> tickets = CreateSampleTickets();

Console.WriteLine($"Liczba zgłoszeń: {tickets.Count}");
DisplayTickets(tickets);

List<Ticket> urgentTickets = GetUrgentTickets(tickets);

bool hasCriticalTicket = HasCriticalTicket(tickets);

Console.WriteLine($"Czy istnieje zgłoszenie krytyczne: {hasCriticalTicket}");

int openTicketCount = CountOpenTickets(tickets);

Console.WriteLine($"Liczba otwartych zgłoszeń: {openTicketCount}");

int countUrgentTickets = CountUrgentTickets(tickets);

Console.WriteLine($"Liczba pilnych zgłoszeń: {countUrgentTickets}");

bool hasInProgressTicket = HasInProgressTicket(tickets);
Console.WriteLine($"Czy istnieje zgłoszenie w toku: {hasInProgressTicket}");

List<Ticket> ticketsByPriority = SortTicketsByPriority(tickets);
DisplayTickets(ticketsByPriority);

List<Ticket> closedTicket = GetClosedTickets(tickets);

Console.WriteLine($"Liczba zamkniętych zgłoszeń: {closedTicket.Count}");

bool hasClosedTicket = HasClosedTicket(tickets);
Console.WriteLine($"Czy istnieje zamknięte zgłoszenie: {hasClosedTicket}");

bool hasTicketRequiringImmediateAttention = HasTicketRequiringImmediateAttention(tickets);
Console.WriteLine($"Czy istnieje zgłoszenie wymagające natychmiastowej reakcji: {hasTicketRequiringImmediateAttention}");

int countTicketsRequiringImmediateAttention = CountTicketsRequiringImmediateAttention(tickets);
Console.WriteLine($"Liczba zgłoszeń wymagających natychmiastowej reakcji: {countTicketsRequiringImmediateAttention}");

int countInProgressTickets = CountInProgressTickets(tickets);
Console.WriteLine($"Zgłoszenia w toku: {countInProgressTickets}");

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
    int openTicketCount =
     ticketsToCheck.Count(ticket => ticket.IsOpen());
    return openTicketCount;
}

static List<Ticket> SortTicketsByPriority(List<Ticket> ticketsToSort)
{
    List<Ticket> ticketsByPriority =
     ticketsToSort.OrderByDescending(ticket => ticket.Priority)
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
        "Closed"
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
    Ticket fifthTicket = new Ticket(
    id: 5,
    title: "Problem z adresem dostawy",
    description: "Użytkownik chce poprawić adres przed wysyłką",
    priority: 1,
    status: "Open"
    );


    List<Ticket> sampleTickets = new List<Ticket>
    {
    firstTicket,
    secondTicket,
    thirdTicket,
    fourthTicket,
    fifthTicket
    };
    return sampleTickets;
}

bool wasFirstTicketClosed = tickets[0].TryClose();
Console.WriteLine($"Zamknięcie otwartego zgłoszenia: {wasFirstTicketClosed}");
Console.WriteLine($"Status: {tickets[0].Status}");
bool wasSecondTicketClosed = tickets[1].TryClose();
Console.WriteLine($"Ponowne zamknięcie zgłoszenia: {wasSecondTicketClosed}");
Console.WriteLine($"Status: {tickets[1].Status}");

int countOpenTicketsAfterChange = CountOpenTickets(tickets);
Console.WriteLine($"Liczba otwartych zgłoszeń po zmianie: {countOpenTicketsAfterChange}");

bool startOpenTickets = tickets[4].TryStartProgress();
Console.WriteLine($"Rozpoczęcie obsługi otwartego zgłoszenia: {startOpenTickets}");
Console.WriteLine($"Status: {tickets[4].Status}");

bool startInProgressTickets = tickets[2].TryStartProgress();
Console.WriteLine($"Ponowne rozpoczęcie obsługi: {startInProgressTickets}");
Console.WriteLine($"Status: {tickets[2].Status}");

static int CountUrgentTickets(List<Ticket> tickets)
{

    int countUrgentTicket =
     tickets.Count(ticket => ticket.IsUrgent());
    return countUrgentTicket;
}

static bool HasInProgressTicket(List<Ticket> ticketsToCheck)
{
    bool result =
    ticketsToCheck.Any(ticket => ticket.IsInProgress());
    return result;
}

static List<Ticket> GetClosedTickets(List<Ticket> ticketsToFilter)
{
    List<Ticket> closedTicket = ticketsToFilter
    .Where(ticket => !ticket.IsOpen())
    .ToList();
    return closedTicket;
}

static bool HasClosedTicket(List<Ticket> ticketsToFilter)
{
    bool result = ticketsToFilter
    .Any(ticket => !ticket.IsOpen());
    return result;
}

static bool HasTicketRequiringImmediateAttention(List<Ticket> ticketsToCheck)
{
    bool result = ticketsToCheck
    .Any(ticket => ticket.RequiresImmediateAttention());
    return result;
}

static int CountInProgressTickets(List<Ticket> ticketsToCheck)
{
    int countTickets = ticketsToCheck
        .Count(ticket => ticket.IsInProgress());
    return countTickets;
}

static int CountTicketsRequiringImmediateAttention(List<Ticket> ticketsToFilter)
{
    int countTickets = ticketsToFilter
    .Count(ticket => ticket.RequiresImmediateAttention());
    return countTickets;
}
