SampleTicketData sampleData = new SampleTicketData();
List<Ticket> tickets = sampleData.CreateSampleTickets();
TicketQueries ticketQueries = new TicketQueries();
TicketConsoleView ticketView = new TicketConsoleView();
TicketConsoleApplication application = new TicketConsoleApplication();
application.Run(tickets, ticketQueries, ticketView);
