public class TicketOperationResult
{
    public TicketOperationStatus Status { get; }
    public Ticket? Ticket { get; }

    public TicketOperationResult(
        TicketOperationStatus status, Ticket? ticket)
    {
        Status = status;
        Ticket = ticket;
    }
}