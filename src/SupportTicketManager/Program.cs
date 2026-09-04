List<Ticket> tickets = CreateSampleTickets();
static Ticket? FindTicketById(List<Ticket> ticketsToSearch, int ticketId)
{
    return ticketsToSearch.FirstOrDefault(ticket => ticket.Id == ticketId);
}
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
            switch (selectedOption)
            {
                case 1:
                    List<Ticket> sortedTickets = SortTicketsByPriority(tickets);
                    DisplayTickets(sortedTickets);
                    break;
                case 2:
                    List<Ticket> urgentTickets = GetUrgentTickets(tickets);
                    DisplayTickets(urgentTickets);
                    break;
                case 3:
                    DisplayBasicQueueSummary(tickets);
                    break;
                case 4:
                    Console.WriteLine("Podaj ID zgłoszenia:");
                    string? ticketIdInput = Console.ReadLine();
                    bool wasTicketIdParsed = int.TryParse(ticketIdInput, out int ticketIdToStart);
                    if (wasTicketIdParsed)
                    {
                        Ticket? ticketToStart = FindTicketById(tickets, ticketIdToStart);
                        if (ticketToStart == null)
                        {
                            Console.WriteLine("Nie znaleziono zgłoszenia.");
                        }
                        else
                        {
                            bool wasStarted = ticketToStart.TryStartProgress();
                            if (wasStarted)
                            {
                                Console.WriteLine("Rozpoczęto obsługę zgłoszenia.");
                            }
                            else
                            {
                                Console.WriteLine("Nie można rozpocząć obsługi tego zgłoszenia.");
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy numer zgłoszenia.");
                    }
                    break;
                case 5:
                    Console.WriteLine("Podaj ID zgłoszenia:");
                    string? ticketIdInputToClose = Console.ReadLine();
                    bool wasTicketIdParsedToClose = int.TryParse(ticketIdInputToClose, out int ticketIdToClose);
                    if (wasTicketIdParsedToClose)
                    {
                        Ticket? ticketToClose = FindTicketById(tickets, ticketIdToClose);
                        if (ticketToClose == null)
                        {
                            Console.WriteLine("Nie znaleziono zgłoszenia.");

                        }
                        else
                        {
                            bool wasClosed = ticketToClose.TryClose();
                            if (wasClosed)
                            {
                                Console.WriteLine("Zamknięto zgłoszenie.");
                            }
                            else
                            {
                                Console.WriteLine("Nie można zamknąć tego zgłoszenia.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy numer zgłoszenia.");
                    }
                    break;
                case 6:
                    Console.WriteLine("Podaj ID zgłoszenia:");
                    string? ticketIdInputToReopen = Console.ReadLine();
                    bool wasTicketIdParsedToReopen = int.TryParse(ticketIdInputToReopen, out int ticketIdToReopen);
                    if (wasTicketIdParsedToReopen)
                    {

                        Ticket? ticketToReopen = FindTicketById(tickets, ticketIdToReopen);

                        if (ticketToReopen == null)
                        {
                            Console.WriteLine("Nie znaleziono zgłoszenia.");

                        }
                        else
                        {
                            bool wasReopened = ticketToReopen.TryReopen();
                            if (wasReopened)
                            {
                                Console.WriteLine("Ponownie otwarto zgłoszenie.");
                            }
                            else
                            {
                                Console.WriteLine("Nie można ponownie otworzyć tego zgłoszenia.");
                            }


                        }


                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy numer zgłoszenia.");
                    }
                    break;
                case 7:
                    Console.WriteLine("Podaj ID zgłoszenia:");
                    string? ticketIdInputToChangePriority = Console.ReadLine();
                    bool wasTicketIdParsedToChangePriority = int.TryParse(ticketIdInputToChangePriority, out int ticketIdToChangePriority);
                    if (wasTicketIdParsedToChangePriority)
                    {
                        Ticket? ticketToChangePriority = FindTicketById(tickets, ticketIdToChangePriority);
                        if (ticketToChangePriority == null)
                        {
                            Console.WriteLine("Nie znaleziono zgłoszenia.");
                        }
                        else
                        {
                            Console.WriteLine("Podaj nowy priorytet (1-5)");
                            string? newPriorityInput = Console.ReadLine();
                            bool wasNewPriorityParsed = int.TryParse(newPriorityInput, out int newPriority);
                            if (wasNewPriorityParsed)
                            {
                                bool wasPriorityChanged = ticketToChangePriority.TryChangePriority(newPriority);
                                if (wasPriorityChanged)
                                {
                                    Console.WriteLine("Zmieniono priorytet zgłoszenia.");
                                }
                                else
                                {
                                    Console.WriteLine("Priorytet musi mieścić się w zakresie 1-5, nie zmieniono priorytetu zgłoszenia.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Nieprawidłowy numer priorytetu.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy numer zgłoszenia.");
                    }
                    break;
                default:
                    Console.WriteLine("Nieobsługiwana opcja.");
                    break;
            }



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
