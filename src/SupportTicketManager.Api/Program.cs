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
builder.Services.AddAuthorization();
var app = builder.Build();
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
    version = "0.7.0"
});
app.MapGet("api/name", () => "Support Ticket Manager");

TicketEndpoints.MapTicketEndpoints(app);
AuthEndpoints.MapAuthEndpoints(app);
app.Run();
public partial class Program
{

}
