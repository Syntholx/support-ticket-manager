public class TicketService
{
    private readonly List<Ticket> tickets;
    public TicketService(List<Ticket> initialTickets)
    {
        tickets = initialTickets;
    }
    public int CountTickets()
    {
        int countTicket = tickets.Count;
        return countTicket;
    }
    public int GetNextTicketId()
    {
        if (tickets.Count == 0)
        {
            return 1;
        }
        int nextId = tickets.Max(ticket => ticket.Id) + 1;
        return nextId;
    }
    public Ticket CreateTicket(string title, string description, int priority)
    {
        int nextTicketId = GetNextTicketId();
        Ticket newTicket = new Ticket(nextTicketId, title, description, priority, TicketStatus.Open);
        tickets.Add(newTicket);
        return newTicket;
    }
}
