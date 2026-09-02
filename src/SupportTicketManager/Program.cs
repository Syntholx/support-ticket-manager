List<Ticket> tickets = CreateSampleTickets();
bool isProgramStillActive = true;
while (isProgramStillActive)
{
    DisplayMainMenu();
    string? optionInput = Console.ReadLine();
    bool wasOptionParsed = int.TryParse(optionInput, out int selectedOption);
    if (wasOptionParsed)
    {
        if (selectedOption == 0)
        {
            isProgramStillActive = false;
            Console.WriteLine("Zamykanie programu...");
        }
        else
        {
            Console.WriteLine($"Wybrano opcję: {selectedOption}");
        }

    }
    else
    {
        Console.WriteLine("Nieprawidłowy numer opcji.");
    }





}
static void DisplayMainMenu()
{
    Console.WriteLine("--- Support Ticket Manager ---");
    Console.WriteLine("1. Pokaż wszystkie zgłoszenia");
    Console.WriteLine("2. Pokaż pilne zgłoszenia");
    Console.WriteLine("3. Pokaż podsumowanie kolejki");
    Console.WriteLine("4. Rozpocznij obsługę zgłoszenia");
    Console.WriteLine("5. Zamknij zgłoszenie");
    Console.WriteLine("6. Otwórz ponownie zgłoszenie");
    Console.WriteLine("7. Zmień priorytet");
    Console.WriteLine("0. Zakończ program");
    Console.WriteLine("Wybierz operację:");
}
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

static void RunTicketStateDemo(List<Ticket> ticketsToTest)
{
    bool wasFirstTicketClosed = ticketsToTest[0].TryClose();
    Console.WriteLine($"Zamknięcie otwartego zgłoszenia: {wasFirstTicketClosed}");
    Console.WriteLine($"Status: {ticketsToTest[0].Status}");
    bool wasSecondTicketClosed = ticketsToTest[1].TryClose();
    Console.WriteLine($"Ponowne zamknięcie zgłoszenia: {wasSecondTicketClosed}");
    Console.WriteLine($"Status: {ticketsToTest[1].Status}");

    int countOpenTicketsAfterChange = CountOpenTickets(ticketsToTest);
    Console.WriteLine($"Liczba otwartych zgłoszeń po zmianie: {countOpenTicketsAfterChange}");

    bool startOpenTickets = ticketsToTest[4].TryStartProgress();
    Console.WriteLine($"Rozpoczęcie obsługi otwartego zgłoszenia: {startOpenTickets}");
    Console.WriteLine($"Status: {ticketsToTest[4].Status}");

    bool startInProgressTickets = ticketsToTest[2].TryStartProgress();
    Console.WriteLine($"Ponowne rozpoczęcie obsługi: {startInProgressTickets}");
    Console.WriteLine($"Status: {ticketsToTest[2].Status}");

    bool reopenClosedTicket = ticketsToTest[1].TryReopen();
    Console.WriteLine($"Ponowne otwarcie zamkniętego zgłoszenia: {reopenClosedTicket}");
    Console.WriteLine($"Status: {ticketsToTest[1].Status}");

    bool reopenInProgressTicket = ticketsToTest[2].TryReopen();
    Console.WriteLine($"Ponowne otwarcie zgłoszenia w toku: {reopenInProgressTicket}");
    Console.WriteLine($"Status: {ticketsToTest[2].Status}");

    bool changePriority = ticketsToTest[4].TryChangePriority(3);
    Console.WriteLine($"Zmiana priorytetu na 3: {changePriority}");
    Console.WriteLine($"Priorytet: {ticketsToTest[4].Priority}");
    bool changePriorityInvalid = ticketsToTest[4].TryChangePriority(6);
    Console.WriteLine($"Zmiana priorytetu na 6: {changePriorityInvalid}");
    Console.WriteLine($"Priorytet: {ticketsToTest[4].Priority}");
}

static void DisplayBasicQueueSummary(List<Ticket> ticketsToSummarize)
{
    bool hasCriticalTicket = HasCriticalTicket(ticketsToSummarize);

    Console.WriteLine($"Czy istnieje zgłoszenie krytyczne: {hasCriticalTicket}");

    int openTicketCount = CountOpenTickets(ticketsToSummarize);

    Console.WriteLine($"Liczba otwartych zgłoszeń: {openTicketCount}");

    int countUrgentTickets = CountUrgentTickets(ticketsToSummarize);

    Console.WriteLine($"Liczba pilnych zgłoszeń: {countUrgentTickets}");

    bool hasInProgressTicket = HasInProgressTicket(ticketsToSummarize);
    Console.WriteLine($"Czy istnieje zgłoszenie w toku: {hasInProgressTicket}");

    List<Ticket> closedTicket = GetClosedTickets(ticketsToSummarize);

    Console.WriteLine($"Liczba zamkniętych zgłoszeń: {closedTicket.Count}");

    bool hasClosedTicket = HasClosedTicket(ticketsToSummarize);
    Console.WriteLine($"Czy istnieje zamknięte zgłoszenie: {hasClosedTicket}");

    bool hasTicketRequiringImmediateAttention = HasTicketRequiringImmediateAttention(ticketsToSummarize);
    Console.WriteLine($"Czy istnieje zgłoszenie wymagające natychmiastowej reakcji: {hasTicketRequiringImmediateAttention}");

    int countTicketsRequiringImmediateAttention = CountTicketsRequiringImmediateAttention(ticketsToSummarize);
    Console.WriteLine($"Liczba zgłoszeń wymagających natychmiastowej reakcji: {countTicketsRequiringImmediateAttention}");

    int countInProgressTickets = CountInProgressTickets(ticketsToSummarize);
    Console.WriteLine($"Zgłoszenia w toku: {countInProgressTickets}");
}
