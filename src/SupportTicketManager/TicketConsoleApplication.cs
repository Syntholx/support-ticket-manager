using System.Linq.Expressions;

public class TicketConsoleApplication
{
    public void Run(
        List<Ticket> tickets,
        TicketQueries ticketQueries,
        TicketConsoleView ticketView)
    {
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
                            HandleStartProgress(tickets, ticketQueries);
                            break;
                        case 5:
                            HandleCloseTicket(tickets, ticketQueries);
                            break;
                        case 6:
                            HandleReopenTicket(tickets, ticketQueries);
                            break;
                        case 7:
                            HandleChangePriorityById(tickets, ticketQueries);
                            break;
                        case 8:
                            List<Ticket> closedTicket = ticketQueries.GetClosedTickets(tickets);
                            ticketView.DisplayTickets(closedTicket);
                            break;
                        case 9:
                            ShowTicketById(tickets, ticketQueries, ticketView);
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

    }
    private void HandleStartProgress(
        List<Ticket> tickets,
        TicketQueries ticketQueries)
    {
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
    }
    private void HandleCloseTicket(
        List<Ticket> tickets,
        TicketQueries ticketQueries)
    {
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
    }
    private void HandleReopenTicket(
        List<Ticket> tickets,
        TicketQueries ticketQueries)
    {
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
    }

    private void HandleChangePriorityById(
        List<Ticket> tickets,
        TicketQueries ticketQueries)
    {
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
    }
    private void ShowTicketById(
        List<Ticket> tickets,
        TicketQueries ticketQueries,
        TicketConsoleView ticketView)
    {
        Console.WriteLine("Podaj ID zgłoszenia");
        string? showInputTicket = Console.ReadLine();
        bool wasShowInputTicketPassed = int.TryParse(showInputTicket, out int showInputTicketId);
        if (wasShowInputTicketPassed)
        {
            Ticket? ticketFoundById = ticketQueries.FindTicketById(tickets, showInputTicketId);
            if (ticketFoundById == null)
            {
                Console.WriteLine("Nie znaleziono zgłoszenia");
            }
            else
            {
                ticketView.DisplayTicketDetails(ticketFoundById);
            }

        }
        else
        {
            Console.WriteLine("Nieprawidłowy numer zgłoszenia");
        }

    }

}
