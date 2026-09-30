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
        modelBuilder.Entity<Ticket>().Property(ticket => ticket.Title).HasMaxLength(Ticket.MaxTitleLength);
        modelBuilder.Entity<Ticket>().Property(ticket => ticket.Description).HasMaxLength(Ticket.MaxDescriptionLength);
        modelBuilder.Entity<Ticket>().Property(ticket => ticket.RowVersion).IsRowVersion();
        modelBuilder.Entity<Ticket>().HasIndex(ticket => new { ticket.OwnerId, ticket.Status, ticket.Priority, ticket.Id });
        modelBuilder.Entity<Ticket>()
        .ToTable("Tickets", table =>
        {
            table.HasCheckConstraint("CK_Tickets_Priority", "[Priority] >=1 AND [Priority] <= 5");
            table.HasCheckConstraint("CK_Tickets_DescriptionLength", "DATALENGTH([Description]) <= 10000");
        });

        modelBuilder.Entity<Ticket>()
        .HasOne<ApplicationUser>()
        .WithMany()
        .HasForeignKey(ticket => ticket.OwnerId)
        .IsRequired(false);
    }
}
