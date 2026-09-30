using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace SupportTicketManager.Tests;

public class ProductionConfigurationTests
{
    [Fact]
    public void Production_RejectsWildcardHosts()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["AllowedHosts"] = "*";
        var error = Assert.Throws<InvalidOperationException>(() => builder.AddApiSecurity());
        Assert.Contains("AllowedHosts", error.Message);
    }

    [Theory]
    [InlineData("Encrypt=False;TrustServerCertificate=False")]
    [InlineData("Encrypt=True;TrustServerCertificate=True")]
    public void Production_RejectsUnverifiedSqlTransport(string transport)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["AllowedHosts"] = "tsm.example.com";
        builder.Configuration["ConnectionStrings:TicketDatabase"] = "Server=sql.example.com;Database=TSM;" + transport;
        var error = Assert.Throws<InvalidOperationException>(() => builder.AddApiSecurity());
        Assert.Contains("SQL", error.Message);
    }

    [Fact]
    public void Production_RequiresPersistentKeyDirectory()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration["AllowedHosts"] = "tsm.example.com";
        builder.Configuration["ConnectionStrings:TicketDatabase"] = "Server=sql.example.com;Database=TSM;Encrypt=True;TrustServerCertificate=False";
        builder.Configuration["DataProtection:KeyPath"] = "";
        var error = Assert.Throws<InvalidOperationException>(() => builder.AddApiSecurity());
        Assert.Contains("DataProtection:KeyPath", error.Message);
    }
}
