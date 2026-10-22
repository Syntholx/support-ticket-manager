using Microsoft.Data.SqlClient;

public static class DemoMode
{
    public const string DatabaseName = "SupportTicketManagerDemo";
    // Deliberately public sample credentials. Never reuse for real accounts or infrastructure.
    public const string UserEmail = "user@demo.example";
    public const string SupportEmail = "support@demo.example";
    public const string UserPassword = "Demo-User!2026";
    public const string SupportPassword = "Demo-Support!2026";

    public static bool IsEnabled(IConfiguration configuration) => configuration.GetValue<bool>("Demo:Enabled");

    public static bool IsDemoEmail(string email) =>
        string.Equals(email, UserEmail, StringComparison.OrdinalIgnoreCase)
        || string.Equals(email, SupportEmail, StringComparison.OrdinalIgnoreCase);

    public static void ValidateDatabase(IConfiguration configuration)
    {
        var connection = new SqlConnectionStringBuilder(configuration.GetConnectionString("TicketDatabase"));
        if (!string.Equals(connection.InitialCatalog, DatabaseName, StringComparison.Ordinal))
            throw new InvalidOperationException("Demo mode requires its own SupportTicketManagerDemo database.");
    }

    public static void UseDemoRestrictions(this WebApplication app)
    {
        if (!IsEnabled(app.Configuration)) return;
        // An allowlist prevents future account-management endpoints from being exposed accidentally.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api/auth")
                && context.Request.Path != "/api/auth/csrf"
                && context.Request.Path != "/api/auth/login"
                && context.Request.Path != "/api/auth/logout"
                && context.Request.Path != "/api/auth/me")
            {
                await Results.Json(new { message = "Ta operacja konta jest wyłączona w publicznym demo." }, statusCode: 403)
                    .ExecuteAsync(context);
                return;
            }
            await next(context);
        });
    }
}
