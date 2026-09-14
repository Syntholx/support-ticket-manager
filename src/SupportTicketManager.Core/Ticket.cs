
public class Ticket
{
    public int Id { get; private set; }
    public string Title { get; private set; } = "";
    public string Description { get; private set; } = "";
    public int Priority { get; private set; }
    public string? OwnerId { get; private set; }
    public TicketStatus Status { get; private set; }

    public Ticket(
        int id,
        string title,
        string description,
        int priority,
        TicketStatus status,
        string? ownerId = null)

    {
        Id = id;
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Tytuł nie może być pusty", nameof(title));
        }
        Title = title;
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Opis nie może być pusty.", nameof(description));
        }
        Description = description;
        if (priority < 1 || priority > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }
        Priority = priority;
        if (status != TicketStatus.Open && status != TicketStatus.InProgress && status != TicketStatus.Closed)
        {
            throw new ArgumentException("Status musi być jednym z: Open, InProgress, Closed", nameof(status));
        }
        Status = status;
        OwnerId = ownerId;
    }

    public bool IsUrgent()
    {
        return Priority >= 4;
    }
    public bool IsCritical()
    {
        return Priority == 5;
    }
    public bool IsOpen()
    {
        return Status != TicketStatus.Closed;
    }
    public bool IsInProgress()
    {
        return Status == TicketStatus.InProgress;
    }
    public bool RequiresImmediateAttention()
    {
        return IsCritical() && IsOpen();
    }
    public bool TryClose()
    {
        if (!IsOpen())
        {
            return false;

        }
        Status = TicketStatus.Closed;

        return true;

    }
    public bool TryStartProgress()
    {
        if (Status != TicketStatus.Open)
        {
            return false;
        }
        Status = TicketStatus.InProgress;

        return true;

    }

    public bool TryReopen()
    {
        if (!CanBeReopened())
        {
            return false;
        }

        Status = TicketStatus.Open;
        return true;
    }

    public bool TryChangePriority(int newPriority)
    {
        if (newPriority < 1 || newPriority > 5)
        {
            return false;
        }
        Priority = newPriority;
        return true;
    }
    public bool CanBeReopened()
    {
        if (Status != TicketStatus.Closed)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
