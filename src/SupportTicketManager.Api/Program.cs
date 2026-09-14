using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<TicketDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("TicketDatabase")));
builder.Services.AddScoped<TicketDatabaseService>();
builder.Services
.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TicketDbContext>()
.AddSignInManager();
builder.Services
.AddAuthentication(IdentityConstants.ApplicationScheme)
.AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Local UI only. CORS is not authentication or a production access policy.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddPolicy("LocalTicketView", policy =>
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type")));
}
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SupportOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Support");
    });
});
var app = builder.Build();
if (args.Contains("--grant-support"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "To polecenie jest dostępne tylko w Development.");
    }

    if (args.Length != 2 ||
        args[0] != "--grant-support" ||
        string.IsNullOrWhiteSpace(args[1]))
    {
        throw new InvalidOperationException(
            "Użycie: --grant-support adres-email");
    }

    using IServiceScope scope = app.Services.CreateScope();

    TicketDbContext database =
        scope.ServiceProvider.GetRequiredService<TicketDbContext>();

    string server = database.Database.GetDbConnection().DataSource;

    if (server != "localhost,1433" && server != "127.0.0.1,1433")
    {
        throw new InvalidOperationException(
            "Polecenie może korzystać tylko z lokalnego SQL Server.");
    }

    UserManager<ApplicationUser> userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    RoleManager<IdentityRole> roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await SupportRoleSetup.AssignAsync(
        args[1], userManager, roleManager);

    Console.WriteLine("Konto ma przypisaną rolę Support.");
    return;
}
if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalTicketView");
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("api/status", () => new
{
    name = "Support Ticket Manager",
    isRunning = true,
    version = "1.0.0"
});
app.MapGet("api/name", () => "Support Ticket Manager");

TicketEndpoints.MapTicketEndpoints(app);
AuthEndpoints.MapAuthEndpoints(app);
app.Run();
public partial class Program
{

}
