public class TicketConsoleView
{
    public void DisplayBasicQueueSummary(List<Ticket> ticketsToSummarize, TicketQueries queries)
    {
        bool hasCriticalTicket = queries.HasCriticalTicket(ticketsToSummarize);

        Console.WriteLine($"Czy istnieje zgłoszenie krytyczne: {hasCriticalTicket}");

        int openTicketCount = queries.CountOpenTickets(ticketsToSummarize);

        Console.WriteLine($"Liczba otwartych zgłoszeń: {openTicketCount}");

        int countUrgentTickets = queries.CountUrgentTickets(ticketsToSummarize);

        Console.WriteLine($"Liczba pilnych zgłoszeń: {countUrgentTickets}");

        bool hasInProgressTicket = queries.HasInProgressTicket(ticketsToSummarize);
        Console.WriteLine($"Czy istnieje zgłoszenie w toku: {hasInProgressTicket}");

        List<Ticket> closedTicket = queries.GetClosedTickets(ticketsToSummarize);

        Console.WriteLine($"Liczba zamkniętych zgłoszeń: {closedTicket.Count}");

        bool hasClosedTicket = queries.HasClosedTicket(ticketsToSummarize);
        Console.WriteLine($"Czy istnieje zamknięte zgłoszenie: {hasClosedTicket}");

        bool hasTicketRequiringImmediateAttention = queries.HasTicketRequiringImmediateAttention(ticketsToSummarize);
        Console.WriteLine($"Czy istnieje zgłoszenie wymagające natychmiastowej reakcji: {hasTicketRequiringImmediateAttention}");

        int countTicketsRequiringImmediateAttention = queries.CountTicketsRequiringImmediateAttention(ticketsToSummarize);
        Console.WriteLine($"Liczba zgłoszeń wymagających natychmiastowej reakcji: {countTicketsRequiringImmediateAttention}");

        int countInProgressTickets = queries.CountInProgressTickets(ticketsToSummarize);
        Console.WriteLine($"Zgłoszenia w toku: {countInProgressTickets}");

    }
    public void DisplayTickets(List<Ticket> ticketsToDisplay)
    {
        foreach (Ticket ticket in ticketsToDisplay)
        {
            Console.WriteLine($"#{ticket.Id} | {ticket.Title} | Priority: {ticket.Priority} | {ticket.Status}");
        }
        if (ticketsToDisplay.Count == 0)
        {
            Console.WriteLine("Brak zgłoszeń do wyświetlenia.");
        }
    }

    public void DisplayMainMenu()
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
}