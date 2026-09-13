using Microsoft.EntityFrameworkCore;
public class TicketDatabaseService
{
    private readonly TicketDbContext dbContext;

    public TicketDatabaseService(TicketDbContext context)
    {
        dbContext = context;
    }
    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync(id);
        return foundTicket;
    }
    public async Task<Ticket> CreateTicketAsync(
        string title, string description, int priority)
    {
        Ticket newTicket = new Ticket(0,
    title,
    description,
   priority,
    TicketStatus.Open);
        dbContext.Tickets.Add(newTicket);
        await dbContext.SaveChangesAsync();
        return newTicket;
    }

    public async Task<List<Ticket>> GetActiveTicketsAsync()
    {
        List<Ticket> tickets = await dbContext.Tickets
        .Where(ticket => ticket.Status != TicketStatus.Closed)
        .OrderByDescending(ticket => ticket.Priority)
        .ToListAsync();

        return tickets;
    }
    public async Task<TicketOperationResult> CloseTicketAsync(int id)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync(id);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }
        bool wasClosed = foundTicket.TryClose();

        if (wasClosed == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }

        await dbContext.SaveChangesAsync();
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }
    public async Task<TicketOperationResult> ReopenTicketAsync(int id)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync(id);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }
        bool wasReopenTickets = foundTicket.TryReopen();
        if (wasReopenTickets == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }
        await dbContext.SaveChangesAsync();
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }
    public async Task<TicketOperationResult> StartTicketAsync(int id)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync(id);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }

        bool wasStartProgress = foundTicket.TryStartProgress();

        if (wasStartProgress == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }

        await dbContext.SaveChangesAsync();
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);

    }
    public async Task<TicketOperationResult> ChangePriorityAsync(int id, int newPriority)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync(id);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }

        bool wasChangePriority = foundTicket.TryChangePriority(newPriority);
        if (wasChangePriority == false)
        {
            return new TicketOperationResult(TicketOperationStatus.InvalidPriority, foundTicket);
        }

        await dbContext.SaveChangesAsync();
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }

    public async Task<List<Ticket>> GetArchivedTicketsAsync()
    {
        List<Ticket> tickets = await dbContext.Tickets
        .Where(ticket => ticket.Status == TicketStatus.Closed)
        .ToListAsync();
        return tickets;

    }
}
