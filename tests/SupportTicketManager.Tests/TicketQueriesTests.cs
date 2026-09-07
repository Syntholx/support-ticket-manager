namespace SupportTicketManager.Tests;

public class TicketQueriesTests
{
    [Fact]
    public void GetActiveTickets_MixedStatuses_ReturnsOpenAndInProgress()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(1, "Test", "Opis testowy", 3, TicketStatus.Open));
        tickets.Add(new Ticket(2, "Kolejny Test", "Opis kolejnego", 5, TicketStatus.Closed));
        tickets.Add(new Ticket(3, "Kolejny test", "Testowy", 2, TicketStatus.InProgress));
        TicketQueries ticketQueries = new TicketQueries();
        List<Ticket> activeTickets = ticketQueries.GetActiveTickets(tickets);
        Assert.Equal(2, activeTickets.Count);
        Assert.Equal(3, tickets.Count);
        Assert.Equal(1, activeTickets[0].Id);
        Assert.Equal(3, activeTickets[1].Id);

    }
    [Fact]
    public void GetClosedTickets_MixedStatuses_ReturnsOnlyClosed()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(1, "Test", "Opis testowy", 3, TicketStatus.Open));
        tickets.Add(new Ticket(2, "Kolejny Test", "Opis kolejnego", 5, TicketStatus.Closed));
        tickets.Add(new Ticket(3, "Kolejny test", "Testowy", 2, TicketStatus.InProgress));
        TicketQueries ticketQueries = new TicketQueries();
        List<Ticket> closedTickets = ticketQueries.GetClosedTickets(tickets);
        Assert.Single(closedTickets);
        Assert.Equal(3, tickets.Count);
        Assert.Equal(2, closedTickets[0].Id);
    }
    [Fact]
    public void FindTicketById_ExtisingId_ReturnsMatchingTicket()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(2, "Logowanie", "Problem z logowaniem", 3, TicketStatus.Open));
        tickets.Add(new Ticket(7, "Płatność", "Problem z płatnością", 5, TicketStatus.Closed));
        TicketQueries ticketQueries = new TicketQueries();
        Ticket? findTickets = ticketQueries.FindTicketById(tickets, 7);
        Assert.Equal(2, tickets.Count);
        Assert.NotNull(findTickets);
        Assert.Equal(7, findTickets.Id);
    }
    [Fact]
    public void FindTicketById_MissingId_ReturnsNull()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(2, "Logowanie", "Problem z logowaniem", 3, TicketStatus.Open));
        tickets.Add(new Ticket(7, "Płatność", "Problem z płatnością", 5, TicketStatus.Closed));
        TicketQueries ticketQueries = new TicketQueries();
        Ticket? findTickets = ticketQueries.FindTicketById(tickets, 99);
        Assert.Equal(2, tickets.Count);
        Assert.Null(findTickets);
    }
    [Fact]
    public void SortTicketsByPriority_ReturnsDescendingOrderAndKeepsSourceUnchange()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(2, "Test", "Opis testowy", 3, TicketStatus.Open));
        tickets.Add(new Ticket(7, "Test", "Opis testowy", 5, TicketStatus.Open));
        tickets.Add(new Ticket(9, "Test", "Opis testowy", 1, TicketStatus.Open));
        TicketQueries ticketQueries = new TicketQueries();
        List<Ticket> sortedList = ticketQueries.SortTicketsByPriority(tickets);
        Assert.Equal(3, sortedList.Count);
        Assert.Equal(3, tickets.Count);
        Assert.Equal(7, sortedList[0].Id);
        Assert.Equal(2, sortedList[1].Id);
        Assert.Equal(9, sortedList[2].Id);
        Assert.Equal(2, tickets[0].Id);
        Assert.Equal(7, tickets[1].Id);
        Assert.Equal(9, tickets[2].Id);
    }
    [Fact]
    public void SortTicketsByPriority_EmptyList_ReturnsEmptyList()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketQueries ticketQueries = new TicketQueries();
        List<Ticket> sortedList = ticketQueries.SortTicketsByPriority(tickets);
        Assert.Empty(tickets);
        Assert.Empty(sortedList);
    }
    [Fact]
    public void GetActiveTickets_AllClosed_ReturnsEmptyList()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(2, "Test", "Test", 3, TicketStatus.Closed));
        tickets.Add(new Ticket(7, "Test", "Test", 5, TicketStatus.Closed));
        TicketQueries ticketQueries = new TicketQueries();
        List<Ticket> activeTicket = ticketQueries.GetActiveTickets(tickets);
        Assert.Empty(activeTicket);
        Assert.Equal(2, tickets.Count);
    }
}