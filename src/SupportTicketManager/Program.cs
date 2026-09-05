List<Ticket> tickets = CreateSampleTickets();
TicketQueries ticketQueries = new TicketQueries();
TicketConsoleView ticketView = new TicketConsoleView();
bool isProgramStillActive = true;
while (isProgramStillActive)
{
    ticketView.DisplayMainMenu();
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
                    List<Ticket> sortedTickets = ticketQueries.SortTicketsByPriority(tickets);
                    ticketView.DisplayTickets(sortedTickets);
                    break;
                case 2:
                    List<Ticket> urgentTickets = ticketQueries.GetUrgentTickets(tickets);
                    ticketView.DisplayTickets(urgentTickets);
                    break;
                case 3:
                    ticketView.DisplayBasicQueueSummary(tickets, ticketQueries);
                    break;
                case 4:
                    Console.WriteLine("Podaj ID zgłoszenia:");
                    string? ticketIdInput = Console.ReadLine();
                    bool wasTicketIdParsed = int.TryParse(ticketIdInput, out int ticketIdToStart);
                    if (wasTicketIdParsed)
                    {
                        Ticket? ticketToStart = ticketQueries.FindTicketById(tickets, ticketIdToStart);
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
                        Ticket? ticketToClose = ticketQueries.FindTicketById(tickets, ticketIdToClose);
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

                        Ticket? ticketToReopen = ticketQueries.FindTicketById(tickets, ticketIdToReopen);

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
                        Ticket? ticketToChangePriority = ticketQueries.FindTicketById(tickets, ticketIdToChangePriority);
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
