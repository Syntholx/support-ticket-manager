using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public interface IAccountEmail
{
    Task SendAsync(string email, string subject, string link, CancellationToken cancellationToken);
}

public sealed class AccountEmailOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string FrontendBaseUrl { get; set; } = "https://localhost:5500";
}

public static class AccountEmailRegistration
{
    public static void AddAccountEmail(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var options = services.AddOptions<AccountEmailOptions>().Bind(configuration.GetSection("Email"))
            .Validate(value => Uri.TryCreate(value.FrontendBaseUrl, UriKind.Absolute, out var uri)
                && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo)
                && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment), "Email:FrontendBaseUrl must be an absolute HTTPS URL without credentials, query or fragment.");
        if (!environment.IsDevelopment())
            options.Validate(value => !string.IsNullOrWhiteSpace(value.Host)
                && value.Port is 465 or 587 && !string.IsNullOrWhiteSpace(value.Username)
                && !string.IsNullOrWhiteSpace(value.Password)
                && Uri.TryCreate(value.FrontendBaseUrl, UriKind.Absolute, out var frontend) && !frontend.IsLoopback
                && MailboxAddress.TryParse(value.From, out _), "Production requires authenticated TLS SMTP configuration.");
        options.ValidateOnStart();
        services.AddScoped<IAccountEmail, SmtpAccountEmail>();
    }
}

public sealed class SmtpAccountEmail(IOptions<AccountEmailOptions> options, IHostEnvironment environment) : IAccountEmail
{
    public async Task SendAsync(string email, string subject, string link, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        bool isLocalMailpit = environment.IsDevelopment()
            && settings.Host == "127.0.0.1" && settings.Port == 1025;

        if (string.IsNullOrWhiteSpace(settings.Host))
            throw new InvalidOperationException("SMTP is not configured. Use a local test sender or configure Email settings.");
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.From));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = $"{subject}\n\n{link}\n\nLink jest ważny przez godzinę. Jeśli nie wysłano tej prośby, zignoruj wiadomość." };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        cancellationToken = deadline.Token;
        using var client = new SmtpClient { Timeout = 15000 };
        var socketOptions = isLocalMailpit ? SecureSocketOptions.None
            : settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await client.ConnectAsync(settings.Host, settings.Port, socketOptions, cancellationToken);
        if (!isLocalMailpit)
            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
