namespace SupportTicketManager.Tests;

public class TicketTests
{
    [Fact]
    public void TryClose_OpenTicket_ReturnsTrueAndClosesTicket()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool ticketClose = ticket.TryClose();
        Assert.True(ticketClose);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }
    [Fact]
    public void TryClose_ClosedTicket_ReturnsFalseAndKeepsStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis Problemu", 3, TicketStatus.Closed);
        bool ticketClose = ticket.TryClose();
        Assert.False(ticketClose);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }
    [Fact]
    public void TryStartProgress_OpenTicket_ReturnsTrueAndChangeStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool ticketStartProgress = ticket.TryStartProgress();
        Assert.True(ticketStartProgress);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }
    [Fact]
    public void TryStartProgress_InProgressTicket_ReturnsFalseAndKeepStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.InProgress);
        bool ticketInProgressStart = ticket.TryStartProgress();
        Assert.False(ticketInProgressStart);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }
    [Fact]
    public void TryStartProgress_ClosedTicket_ReturnsFalseAndKeepStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Closed);
        bool StartProgressClosedTicket = ticket.TryStartProgress();
        Assert.False(StartProgressClosedTicket);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public void TryReopen_ClosedTicket_ReturnsTrueAndOpensTicket()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Closed);
        bool reopenTicket = ticket.TryReopen();
        Assert.True(reopenTicket);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }
    [Fact]
    public void TryReopen_OpenTicket_ReturnsFalseAndKeepsStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool reopenTicket = ticket.TryReopen();
        Assert.False(reopenTicket);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }
    [Fact]
    public void TryReopen_InProgressTicket_ReturnsFalseAndKeepsStatus()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.InProgress);
        bool reopenTicket = ticket.TryReopen();
        Assert.False(reopenTicket);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }
    [Fact]
    public void TryChangePriority_Zero_ReturnsFalseAndKeepsPriority()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool changePriority = ticket.TryChangePriority(0);
        Assert.False(changePriority);
        Assert.Equal(3, ticket.Priority);
    }
    [Fact]
    public void TryChangePriority_One_ReturnsTrueAndChangePriority()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool changePriority = ticket.TryChangePriority(1);
        Assert.True(changePriority);
        Assert.Equal(1, ticket.Priority);
    }
    [Fact]
    public void TryChangePriority_Five_ReturnsTrueAndChangePriority()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool changePriority = ticket.TryChangePriority(5);
        Assert.True(changePriority);
        Assert.Equal(5, ticket.Priority);
    }
    [Fact]
    public void TryChangePriority_Six_ReturnsFalseAndKeepsPriority()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.Open);
        bool changePriority = ticket.TryChangePriority(6);
        Assert.False(changePriority);
        Assert.Equal(3, ticket.Priority);
    }
    [Fact]
    public void TryClose_InProgressTicket_ReturnsTrueAndClosedTicket()
    {
        Ticket ticket = new Ticket(1, "Problem", "Opis problemu", 3, TicketStatus.InProgress);
        bool closedTicket = ticket.TryClose();
        Assert.True(closedTicket);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }
}