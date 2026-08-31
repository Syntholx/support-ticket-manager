

using System.Reflection.Metadata.Ecma335;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int Priority { get; set; }
    public string Status { get; private set; } = "";

    public Ticket(
        int id,
        string title,
        string description,
        int priority,
        string status)
    {
        Id = id;
        Title = title;
        Description = description;
        Priority = priority;
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

}