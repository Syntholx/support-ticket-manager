
public class Ticket
{
    public int Id { get; private set; }
    public string Title { get; private set; } = "";
    public string Description { get; private set; } = "";
    public int Priority { get; private set; }
    public string Status { get; private set; } = "";

    public Ticket(
        int id,
        string title,
        string description,
        int priority,
        string status)
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
        if (status != "Open" && status != "InProgress" && status != "Closed")
        {
            throw new ArgumentException("Status musi być jednym z: Open, InProgress, Closed", nameof(status));
        }
        Status = status;
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
        return Status != "Closed";
    }
    public bool IsInProgress()
    {
        return Status == "InProgress";
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
        Status = "Closed";

        return true;

    }
    public bool TryStartProgress()
    {
        if (Status != "Open")
        {
            return false;
        }
        Status = "InProgress";

        return true;

    }

    public bool TryReopen()
    {
        if (!CanBeReopened())
        {
            return false;
        }

        Status = "Open";
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
        if (Status != "Closed")
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
