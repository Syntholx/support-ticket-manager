using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

public static class AuthEndpoints
{
    private const string GenericEmailMessage = "Jeśli konto wymaga tej operacji, wysłano wiadomość e-mail.";
    private static bool ValidCredentials(string? email, string? password) =>
        !string.IsNullOrWhiteSpace(email) && email.Length <= 254
        && !string.IsNullOrWhiteSpace(password) && password.Length <= 128;

    public static void MapAuthEndpoints(WebApplication app)
    {
        app.MapGet("/api/auth/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }));

        app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<ApplicationUser> users,
            IAccountEmail sender, IOptions<AccountEmailOptions> emailOptions, HttpContext context) =>
        {
            if (!ValidCredentials(request.Email, request.Password))
                return Results.BadRequest(new { message = "Nieprawidłowy e-mail lub hasło." });
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await users.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(new { message = "Nie udało się utworzyć konta" });
            if (!await SendTokenAsync(user, false, users, sender, emailOptions.Value, context))
                return Results.Problem(statusCode: 503, title: "Konto utworzone, ale wysyłka nie powiodła się. Ponów potwierdzenie e-mail.");
            return Results.Json(new { message = "Konto zostało utworzone. Potwierdź adres e-mail." }, statusCode: 201);
        });

        app.MapPost("/api/auth/confirm-email", async (ConfirmEmailRequest request, UserManager<ApplicationUser> users) =>
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.UserId.Length > 450 || !TryDecode(request.Token, out var token))
                return Results.BadRequest(new { message = "Nieprawidłowy lub wygasły link." });
            var user = await users.FindByIdAsync(request.UserId);
            if (user is null || user.EmailConfirmed || !(await users.ConfirmEmailAsync(user, token)).Succeeded)
                return Results.BadRequest(new { message = "Nieprawidłowy lub wygasły link." });
            return Results.Ok(new { message = "Adres e-mail potwierdzony." });
        });

        app.MapPost("/api/auth/resend-confirmation", async (EmailRequest request, UserManager<ApplicationUser> users,
            IAccountEmail sender, IOptions<AccountEmailOptions> options, HttpContext context) =>
                await SendExistingAsync(request, false, users, sender, options.Value, context));
        app.MapPost("/api/auth/forgot-password", async (EmailRequest request, UserManager<ApplicationUser> users,
            IAccountEmail sender, IOptions<AccountEmailOptions> options, HttpContext context) =>
                await SendExistingAsync(request, true, users, sender, options.Value, context));

        app.MapPost("/api/auth/reset-password", async (ResetPasswordRequest request, UserManager<ApplicationUser> users) =>
        {
            if (!ValidCredentials(request.Email, request.Password) || !TryDecode(request.Token, out var token))
                return Results.BadRequest(new { message = "Nieprawidłowe dane lub wygasły link." });
            var user = await users.FindByEmailAsync(request.Email);
            if (user is null || !user.EmailConfirmed || !(await users.ResetPasswordAsync(user, token, request.Password)).Succeeded)
                return Results.BadRequest(new { message = "Nieprawidłowe dane lub wygasły link." });
            return Results.Ok(new { message = "Hasło zmienione. Zaloguj się ponownie." });
        });

        app.MapPost("/api/auth/login", async (LoginRequest request, SignInManager<ApplicationUser> signIn) =>
        {
            if (!ValidCredentials(request.Email, request.Password))
                return Results.BadRequest(new { message = "E-mail i hasło są wymagane (limity: 254 i 128 znaków)." });
            var result = await signIn.PasswordSignInAsync(request.Email, request.Password, false, true);
            return result.Succeeded ? Results.Ok(new { message = "Zalogowano." }) : Results.Unauthorized();
        });
        app.MapGet("/api/auth/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            id = user.FindFirstValue(ClaimTypes.NameIdentifier), name = user.Identity?.Name
        })).RequireAuthorization();
        app.MapPost("/api/auth/logout", async (SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, ClaimsPrincipal principal) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is not null && !(await users.UpdateSecurityStampAsync(user)).Succeeded)
                return Results.Problem(statusCode: 503, title: "Nie udało się unieważnić sesji. Ponów operację.");
            await signIn.SignOutAsync();
            return Results.Ok(new { message = "Wylogowano" });
        }).RequireAuthorization();
    }

    private static async Task<IResult> SendExistingAsync(EmailRequest request, bool reset,
        UserManager<ApplicationUser> users, IAccountEmail sender, AccountEmailOptions options, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 254)
            return Results.BadRequest(new { message = "Nieprawidłowy e-mail." });
        var user = await users.FindByEmailAsync(request.Email);
        if (user is not null && user.EmailConfirmed == reset)
            await SendTokenAsync(user, reset, users, sender, options, context);
        // Same response for missing account, wrong state and delivery failure.
        return Results.Accepted(value: new { message = GenericEmailMessage });
    }

    private static async Task<bool> SendTokenAsync(ApplicationUser user, bool reset, UserManager<ApplicationUser> users,
        IAccountEmail sender, AccountEmailOptions options, HttpContext context)
    {
        var database = context.RequestServices.GetRequiredService<TicketDbContext>();
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddMinutes(-5);
        string claimStamp = Guid.NewGuid().ToString();
        // Atomic database claim prevents email flooding for one account across IPs/instances.
        var candidates = database.Users.Where(account => account.Id == user.Id);
        int claimed = reset
            ? await candidates.Where(account => account.LastPasswordResetEmailAt == null || account.LastPasswordResetEmailAt < cutoff)
                .ExecuteUpdateAsync(update => update.SetProperty(account => account.LastPasswordResetEmailAt, now)
                    .SetProperty(account => account.ConcurrencyStamp, claimStamp), context.RequestAborted)
            : await candidates.Where(account => account.LastConfirmationEmailAt == null || account.LastConfirmationEmailAt < cutoff)
                .ExecuteUpdateAsync(update => update.SetProperty(account => account.LastConfirmationEmailAt, now)
                    .SetProperty(account => account.ConcurrencyStamp, claimStamp), context.RequestAborted);
        if (claimed == 0) return true;
        string token = reset ? await users.GeneratePasswordResetTokenAsync(user) : await users.GenerateEmailConfirmationTokenAsync(user);
        string encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        string route = reset ? "reset-password" : "confirm-email";
        string identity = reset ? "email=" + Uri.EscapeDataString(user.Email!) : "userId=" + Uri.EscapeDataString(user.Id);
        // Fragment keeps the token out of HTTP access logs / Referer. The UI POSTs it with CSRF.
        string link = $"{options.FrontendBaseUrl.TrimEnd('/')}/{route}#{identity}&token={encoded}";
        try
        {
            await sender.SendAsync(user.Email!, reset ? "TSM: reset hasła" : "TSM: potwierdź e-mail", link, context.RequestAborted);
            return true;
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { throw; }
        catch (Exception)
        {
            // Release only our claim on failure, allowing an explicit retry.
            string releaseStamp = Guid.NewGuid().ToString();
            if (reset)
                await candidates.Where(account => account.LastPasswordResetEmailAt == now)
                    .ExecuteUpdateAsync(update => update.SetProperty(account => account.LastPasswordResetEmailAt, (DateTimeOffset?)null)
                        .SetProperty(account => account.ConcurrencyStamp, releaseStamp));
            else
                await candidates.Where(account => account.LastConfirmationEmailAt == now)
                    .ExecuteUpdateAsync(update => update.SetProperty(account => account.LastConfirmationEmailAt, (DateTimeOffset?)null)
                        .SetProperty(account => account.ConcurrencyStamp, releaseStamp));
            context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AccountEmail")
                .LogError("Account email delivery failed. Check SMTP health and retry via the email endpoint.");
            return false;
        }
    }

    private static bool TryDecode(string? value, out string token)
    {
        token = "";
        if (string.IsNullOrWhiteSpace(value) || value.Length > 4096) return false;
        try { token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value)); return true; }
        catch (FormatException) { return false; }
    }
}

public record EmailRequest(string Email);
public record ConfirmEmailRequest(string UserId, string Token);
public record ResetPasswordRequest(string Email, string Token, string Password);
