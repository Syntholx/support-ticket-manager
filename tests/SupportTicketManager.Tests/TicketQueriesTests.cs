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
}