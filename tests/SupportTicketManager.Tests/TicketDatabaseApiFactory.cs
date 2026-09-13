using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SupportTicketManager.Tests;

public class TicketDatabaseFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"SupportTicketManagerTests_{Guid.NewGuid():N}";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<TicketDbContext>>();
            services.RemoveAll<DbContextOptions<TicketDbContext>>();

            services.AddDbContext<TicketDbContext>((provider, options) =>
            {
                IConfiguration configuration =
                    provider.GetRequiredService<IConfiguration>();

                string? connectionString =
                    configuration.GetConnectionString("TicketDatabase");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Brak konfiguracji połączenia dla testów.");
                }

                SqlConnectionStringBuilder testConnection =
                    new SqlConnectionStringBuilder(connectionString);

                if (testConnection.DataSource != "127.0.0.1,1433"
                    && testConnection.DataSource != "localhost,1433")
                {
                    throw new InvalidOperationException(
                        "Testy mogą korzystać tylko z lokalnego SQL Server.");
                }

                testConnection.InitialCatalog = databaseName;

                options.UseSqlServer(testConnection.ConnectionString);
            });
        });
    }
    public async Task InitializeDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();

        TicketDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TicketDbContext>();

        if (dbContext.Database.GetDbConnection().Database != databaseName)
        {
            throw new InvalidOperationException(
                "Przerwano: połączenie nie wskazuje bazy testowej.");
        }

        await dbContext.Database.MigrateAsync();
    }
    public async Task DeleteDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();

        TicketDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TicketDbContext>();

        if (dbContext.Database.GetDbConnection().Database != databaseName)
        {
            throw new InvalidOperationException(
                "Przerwano: nie wolno usunąć bazy innej niż testowa.");
        }

        await dbContext.Database.EnsureDeletedAsync();
    }
}