using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class TicketDbContext : IdentityDbContext<ApplicationUser>
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }
    public DbSet<Ticket> Tickets => Set<Ticket>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Ticket>()
        .ToTable("Tickets", table =>
        table.HasCheckConstraint("CK_Tickets_Priority",
        "[Priority] >=1 AND [Priority] <= 5"));
    }
}