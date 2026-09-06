using System.ComponentModel;

namespace SupportTicketManager.Tests;

public class TicketServiceTests
{
    [Fact]
    public void GetNextTicketId_EmptyList_ReturnsOne()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService service = new TicketService(tickets);

        int nextId = service.GetNextTicketId();

        Assert.Equal(1, nextId);
        Assert.Empty(tickets);
    }

    [Fact]
    public void GetNextTicketId_ExistingTickets_ReturnsHighestIdPlusOne()
    {
        List<Ticket> tickets = new List<Ticket>();

        tickets.Add(new Ticket(2, "Test", "Opis testowy", 3, TicketStatus.Open));
        tickets.Add(new Ticket(7, "Testowy", "Opis", 4, TicketStatus.Open));
        tickets.Add(new Ticket(4, "Teest", "Opis problemu", 3, TicketStatus.Open));

        TicketService ticketService = new TicketService(tickets);
        int nextId = ticketService.GetNextTicketId();
        Assert.Equal(8, nextId);
        Assert.Equal(3, tickets.Count);

    }
    [Fact]
    public void GetNextTicketId_SingleTicket_ReturnsEleven()
    {
        List<Ticket> tickets = new List<Ticket>();
        tickets.Add(new Ticket(10, "Test", "Opis testowy", 3, TicketStatus.Open));
        TicketService serivce = new TicketService(tickets);
        int nextId = serivce.GetNextTicketId();
        Assert.Equal(11, nextId);
        Assert.Single(tickets);
    }
    [Fact]
    public void CreateTicket_EmptyList_AddsOpenTicketWithIdOne()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService service = new TicketService(tickets);
        Ticket createdTicket = service.CreateTicket("Problem", "Opis problemu", 3);
        Assert.Equal(1, createdTicket.Id);
        Assert.Equal(TicketStatus.Open, createdTicket.Status);
        Assert.Single(tickets);
    }
    [Fact]
    public void CreateTicket_CalledTwice_AssignsConsecutiveIds()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService serivce = new TicketService(tickets);
        Ticket createdTicket = serivce.CreateTicket("Pierwszy problem", "Opis pierwszego", 2);
        Ticket createdTicketTwo = serivce.CreateTicket("Drugi problem", "Opis drugiego", 4);
        Assert.Equal(1, createdTicket.Id);
        Assert.Equal(2, createdTicketTwo.Id);
        Assert.Equal(2, tickets.Count);
    }
    [Fact]
    public void CreateTicket_InvalidPriority_ThrowsAndLeavesListEmpty()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService service = new TicketService(tickets);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
 {
     service.CreateTicket("Problem", "Opis problemu", 8);
 });
        Assert.Empty(tickets);
    }
    [Fact]
    public void CreateTicket_WhitespaceTitle_ThrowsAndLeavesListEmpty()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService service = new TicketService(tickets);
        Assert.Throws<ArgumentException>(() =>
        {
            service.CreateTicket("   ", "Opis problemu", 3);
        });
        Assert.Empty(tickets);
    }
    [Fact]
    public void CreateTicket_WhitespacesDescription_ThrowsAndLeavesListEmpty()
    {
        List<Ticket> tickets = new List<Ticket>();
        TicketService service = new TicketService(tickets);
        Assert.Throws<ArgumentException>(() =>
        {
            service.CreateTicket("Problem", "   ", 3);
        });
        Assert.Empty(tickets);
    }
    [Fact]
    public void CreateTicket_InvalidPriority_PreservesExtingTicket()
    {
        List<Ticket> tickets = new List<Ticket>();

        tickets.Add(new Ticket(10, "Istniejące", "Opis istniejącego", 3, TicketStatus.Open));

        TicketService service = new TicketService(tickets);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            service.CreateTicket("Nowe", "Opis nowego", 8);
        });
        Assert.Single(tickets);
        Assert.Equal(10, tickets[0].Id);
    }
}
