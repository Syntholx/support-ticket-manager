using Microsoft.EntityFrameworkCore;
public class TicketDatabaseService
{
    private readonly TicketDbContext dbContext;

    public TicketDatabaseService(TicketDbContext context)
    {
        dbContext = context;
    }
    public async Task<Ticket?> GetTicketByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync([id], cancellationToken);
        return foundTicket;
    }
    public async Task<Ticket> CreateTicketAsync(
        string title, string description, string ownerId, CancellationToken cancellationToken = default)
    {
        Ticket newTicket = new Ticket(0,
    title,
    description,
    2,
   TicketStatus.Open,
   ownerId);


        dbContext.Tickets.Add(newTicket);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newTicket;
    }

    public async Task<List<Ticket>> GetActiveTicketsAsync(string ownerId, bool isSupport, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {

        List<Ticket> tickets = await dbContext.Tickets
        .Where(ticket => ticket.Status != TicketStatus.Closed && (ticket.OwnerId == ownerId || isSupport))
        .OrderByDescending(ticket => ticket.Priority)
        .ThenBy(ticket => ticket.Id)
        .AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize)
        .ToListAsync(cancellationToken);

        return tickets;
    }
    public async Task<TicketOperationResult> CloseTicketAsync(int id, string ownerId, bool isSupport, CancellationToken cancellationToken = default)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync([id], cancellationToken);

        if (foundTicket == null || (foundTicket.OwnerId != ownerId && !isSupport))
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }
        bool wasClosed = foundTicket.TryClose();

        if (wasClosed == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }
    public async Task<TicketOperationResult> ReopenTicketAsync(int id, CancellationToken cancellationToken = default)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync([id], cancellationToken);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }
        bool wasReopenTickets = foundTicket.TryReopen();
        if (wasReopenTickets == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }
    public async Task<TicketOperationResult> StartTicketAsync(int id, CancellationToken cancellationToken = default)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync([id], cancellationToken);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }

        bool wasStartProgress = foundTicket.TryStartProgress();

        if (wasStartProgress == false)
        {
            return new TicketOperationResult(TicketOperationStatus.Conflict, foundTicket);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);

    }
    public async Task<TicketOperationResult> ChangePriorityAsync(int id, int newPriority, CancellationToken cancellationToken = default)
    {
        Ticket? foundTicket = await dbContext.Tickets.FindAsync([id], cancellationToken);
        if (foundTicket == null)
        {
            return new TicketOperationResult(TicketOperationStatus.NotFound, null);
        }

        bool wasChangePriority = foundTicket.TryChangePriority(newPriority);
        if (wasChangePriority == false)
        {
            return new TicketOperationResult(TicketOperationStatus.InvalidPriority, foundTicket);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new TicketOperationResult(TicketOperationStatus.Success, foundTicket);
    }

    public async Task<List<Ticket>> GetArchivedTicketsAsync(string ownerId, bool isSupport, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        List<Ticket> tickets = await dbContext.Tickets
       .Where(ticket => ticket.Status == TicketStatus.Closed && (isSupport || ticket.OwnerId == ownerId))
        .OrderBy(ticket => ticket.Id)
        .AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize)
        .ToListAsync(cancellationToken);
        return tickets;

    }
}
