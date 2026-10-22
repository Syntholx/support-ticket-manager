using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DemoDatabase
{
    public static async Task RunAsync(WebApplication app, string action)
    {
        if (!DemoMode.IsEnabled(app.Configuration) || action is not ("initialize" or "reset"))
            throw new InvalidOperationException("Use --demo-action initialize|reset only with Demo:Enabled=true.");
        DemoMode.ValidateDatabase(app.Configuration);
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        // A second guard checks the actual context, not just configuration.
        if (db.Database.GetDbConnection().Database != DemoMode.DatabaseName)
            throw new InvalidOperationException("Refusing to modify a database other than SupportTicketManagerDemo.");
        if (action == "initialize") await db.Database.MigrateAsync();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await db.Database.ExecuteSqlRawAsync("SELECT COUNT_BIG(*) FROM dbo.Tickets WITH (TABLOCKX, HOLDLOCK)");
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var existing = await db.Users.ToListAsync();
        if (existing.Any(user => user.Email is null || !DemoMode.IsDemoEmail(user.Email)))
            throw new InvalidOperationException("Unexpected accounts: refusing to reset demo data.");
        if (existing.Count == 0 && action == "initialize")
        {
            if (await db.Tickets.AnyAsync() || await db.Roles.AnyAsync())
                throw new InvalidOperationException("Demo initialization requires an empty database.");
            Require(await roles.CreateAsync(new IdentityRole("Support")));
            foreach (var (email, password) in new[] {
                (DemoMode.UserEmail, DemoMode.UserPassword), (DemoMode.SupportEmail, DemoMode.SupportPassword) })
            {
                var account = await CreateAccountAsync(users, email, password);
                if (email == DemoMode.SupportEmail) Require(await users.AddToRoleAsync(account, "Support"));
            }
        }
        var regular = await users.FindByEmailAsync(DemoMode.UserEmail);
        var support = await users.FindByEmailAsync(DemoMode.SupportEmail);
        if (await db.Users.CountAsync() != 2 || regular is null || support is null
            || !regular.EmailConfirmed || !support.EmailConfirmed || regular.LockoutEnabled || support.LockoutEnabled
            || !await users.CheckPasswordAsync(regular, DemoMode.UserPassword)
            || !await users.CheckPasswordAsync(support, DemoMode.SupportPassword)
            || (await users.GetRolesAsync(regular)).Count != 0
            || !(await users.GetRolesAsync(support)).SequenceEqual(new[] { "Support" }))
            throw new InvalidOperationException("Unexpected demo account state: reset aborted.");

        await db.Tickets.ExecuteDeleteAsync();
        db.Tickets.AddRange(CreateTickets(regular.Id, support.Id));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        app.Logger.LogInformation("Demo data restored: 12 fictional tickets; accounts preserved.");
    }

    public static async Task<ApplicationUser> CreateAccountAsync(UserManager<ApplicationUser> users, string email, string password)
    {
        var account = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, LockoutEnabled = false };
        Require(await users.CreateAsync(account, password));
        // Identity's default AllowedForNewUsers overwrites LockoutEnabled during CreateAsync.
        Require(await users.SetLockoutEnabledAsync(account, false));
        return account;
    }

    private static void Require(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("Demo account setup failed.");
    }

    public static Ticket[] CreateTickets(string userId, string supportId)
    {
        var data = new (string Title, int Priority, TicketStatus Status)[] {
            ("Brak dostępu do panelu po zalogowaniu", 5, TicketStatus.InProgress),
            ("Błąd zapisu formularza zgłoszenia", 4, TicketStatus.Open),
            ("Nie można pobrać faktury PDF", 4, TicketStatus.InProgress),
            ("Powiadomienie e-mail nie dociera", 3, TicketStatus.Open),
            ("Nieprawidłowa data w historii zamówienia", 2, TicketStatus.InProgress),
            ("Długi tytuł wychodzi poza kartę na telefonie", 2, TicketStatus.Open),
            ("Prośba o dodanie podpowiedzi w formularzu", 1, TicketStatus.Open),
            ("Przywrócenie dostępu do konta", 5, TicketStatus.Closed),
            ("Naprawa wyszukiwania zgłoszeń", 3, TicketStatus.Closed),
            ("Poprawa odstępów przycisków na telefonie", 2, TicketStatus.Closed),
            ("Aktualizacja opisu potwierdzenia e-mail", 1, TicketStatus.Closed),
            ("Usunięcie błędu sortowania listy", 4, TicketStatus.Closed),
        };
        return data.Select((item, index) => new Ticket(0, item.Title,
            "Fikcyjne zgłoszenie demonstracyjne.\nOpisuje przykładowy problem i oczekiwany rezultat. Nie zawiera danych prawdziwych osób.",
            item.Priority, item.Status, index == 6 ? supportId : userId)).ToArray();
    }
}
