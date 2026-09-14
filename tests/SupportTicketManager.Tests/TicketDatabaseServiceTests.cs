using Microsoft.Extensions.DependencyInjection;

namespace SupportTicketManager.Tests;

public class TicketDatabaseServiceTests
{
    [Fact]
    public async Task GetActiveTicketsAsync_ExcludesClosedAndSortsByPriority()
    {
        using TicketDatabaseFactory factory = new TicketDatabaseFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using HttpClient client = TicketTestAuthentication.CreateClient(factory);
            string supportId = await TicketTestAuthentication.RegisterSupportAndLoginAsync(factory, client);

            using IServiceScope scope = factory.Services.CreateScope();

            TicketDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<TicketDbContext>();

            TicketDatabaseService service =
                scope.ServiceProvider.GetRequiredService<TicketDatabaseService>();

            Ticket openTicket =
                new Ticket(0, "Otwarte", "Czeka na obsługę", 2, TicketStatus.Open);

            Ticket closedTicket =
                new Ticket(0, "Zamknięte", "Obsługa zakończona", 5, TicketStatus.Closed);

            Ticket inProgressTicket =
                new Ticket(0, "W trakcie", "Trwa obsługa", 4, TicketStatus.InProgress);

            dbContext.Tickets.Add(openTicket);
            dbContext.Tickets.Add(closedTicket);
            dbContext.Tickets.Add(inProgressTicket);
            await dbContext.SaveChangesAsync();

            List<Ticket> result = await service.GetActiveTicketsAsync(supportId, true);

            Assert.Equal(2, result.Count);
            Assert.Equal(inProgressTicket.Id, result[0].Id);
            Assert.Equal(openTicket.Id, result[1].Id);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
}
