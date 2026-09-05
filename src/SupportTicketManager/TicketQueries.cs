public class TicketQueries
{
    public Ticket? FindTicketById(List<Ticket> ticketsToSearch, int ticketId)
    {
        return ticketsToSearch.FirstOrDefault(ticket => ticket.Id == ticketId);
    }

    public List<Ticket> GetUrgentTickets(List<Ticket> ticketsToFilter)
    {
        List<Ticket> filteredTickets = ticketsToFilter
         .Where(ticket => ticket.IsUrgent() && ticket.IsOpen())
         .ToList();
        return filteredTickets;
    }
    public List<Ticket> SortTicketsByPriority(List<Ticket> ticketsToSort)
    {
        List<Ticket> ticketsByPriority =
         ticketsToSort.OrderByDescending(ticket => ticket.Priority)
         .ToList();
        return ticketsByPriority;
    }
    public int CountOpenTickets(List<Ticket> ticketsToCheck)
    {
        int openTicketCount =
         ticketsToCheck.Count(ticket => ticket.IsOpen());
        return openTicketCount;
    }
    public bool HasCriticalTicket(List<Ticket> ticketsToCheck)
    {
        bool result = ticketsToCheck
         .Any(ticket => ticket.IsCritical() && ticket.IsOpen());
        return result;
    }
    public int CountUrgentTickets(List<Ticket> tickets)
    {

        int countUrgentTicket =
         tickets.Count(ticket => ticket.IsUrgent() && ticket.IsOpen());
        return countUrgentTicket;
    }

    public bool HasInProgressTicket(List<Ticket> ticketsToCheck)
    {
        bool result =
        ticketsToCheck.Any(ticket => ticket.IsInProgress());
        return result;
    }

    public List<Ticket> GetClosedTickets(List<Ticket> ticketsToFilter)
    {
        List<Ticket> closedTicket = ticketsToFilter
        .Where(ticket => !ticket.IsOpen())
        .ToList();
        return closedTicket;
    }

    public bool HasClosedTicket(List<Ticket> ticketsToFilter)
    {
        bool result = ticketsToFilter
        .Any(ticket => !ticket.IsOpen());
        return result;
    }

    public bool HasTicketRequiringImmediateAttention(List<Ticket> ticketsToCheck)
    {
        bool result = ticketsToCheck
        .Any(ticket => ticket.RequiresImmediateAttention());
        return result;
    }

    public int CountInProgressTickets(List<Ticket> ticketsToCheck)
    {
        int countTickets = ticketsToCheck
            .Count(ticket => ticket.IsInProgress());
        return countTickets;
    }

    public int CountTicketsRequiringImmediateAttention(List<Ticket> ticketsToFilter)
    {
        int countTickets = ticketsToFilter
        .Count(ticket => ticket.RequiresImmediateAttention());
        return countTickets;
    }
    public int GetNextTicketId(List<Ticket> tickets)
    {
        if (tickets.Count == 0)
        {
            return 1;
        }
        int nextTicketId = tickets.Max(ticket => ticket.Id) + 1;
        return nextTicketId;
    }
    public List<Ticket> GetActiveTickets(List<Ticket> ticketsActive)
    {
        List<Ticket> activeTickets = ticketsActive
        .Where(ticket => ticket.IsOpen())
        .ToList();
        return activeTickets;
    }
}
