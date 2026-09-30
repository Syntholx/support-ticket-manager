using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public static class ApiSecurity
{
    public const int MaxBodyBytes = 32 * 1024;
    public const string CsrfHeader = "X-CSRF-TOKEN";

    public static void AddApiSecurity(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = MaxBodyBytes;
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddRequestTimeouts(options => options.DefaultPolicy = new()
        {
            Timeout = TimeSpan.FromSeconds(30), TimeoutStatusCode = StatusCodes.Status504GatewayTimeout
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = CsrfHeader;
            options.Cookie.Name = "__Host-TSM-CSRF";
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        var protection = builder.Services.AddDataProtection().SetApplicationName("SupportTicketManager");
        if (!builder.Environment.IsDevelopment())
        {
            string Required(string key) => !string.IsNullOrWhiteSpace(builder.Configuration[key])
                ? builder.Configuration[key]! : throw new InvalidOperationException($"Required production setting: {key}");
            string hosts = Required("AllowedHosts");
            if (hosts.Split(';').Any(host => host.Contains('*')))
                throw new InvalidOperationException("Production requires explicit AllowedHosts, without wildcards.");
            var connection = new SqlConnectionStringBuilder(Required("ConnectionStrings:TicketDatabase"));
            if (connection.TrustServerCertificate || connection.Encrypt == SqlConnectionEncryptOption.Optional)
                throw new InvalidOperationException("Production SQL requires encryption and a verified server certificate.");
            string keyPath = Required("DataProtection:KeyPath");
            if (!Path.IsPathFullyQualified(keyPath) || !Directory.Exists(keyPath))
                throw new InvalidOperationException("DataProtection:KeyPath must be an existing absolute persistent directory.");
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                Required("DataProtection:CertificatePath"), Required("DataProtection:CertificatePassword"),
                X509KeyStorageFlags.EphemeralKeySet);
            if (!certificate.HasPrivateKey || certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow
                || certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow)
                throw new InvalidOperationException("Data Protection requires a valid certificate with private key.");
            protection.PersistKeysToFileSystem(new DirectoryInfo(keyPath)).ProtectKeysWithCertificate(certificate);
        }
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (string address in builder.Configuration.GetSection("Security:KnownProxies").Get<string[]>() ?? [])
                options.KnownProxies.Add(IPAddress.Parse(address));
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetConcurrencyLimiter("all",
                    _ => new ConcurrencyLimiterOptions { PermitLimit = 32, QueueLimit = 0 })),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter("ip:" + context.Connection.RemoteIpAddress, _ => Window(120))),
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api/auth") && !HttpMethods.IsGet(context.Request.Method))
                        return RateLimitPartition.GetFixedWindowLimiter("auth:" + context.Connection.RemoteIpAddress, _ => Window(10));
                    string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    return userId is null ? RateLimitPartition.GetNoLimiter("anonymous")
                        : RateLimitPartition.GetFixedWindowLimiter("user:" + userId, _ => Window(60));
                }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                await Results.Problem(statusCode: 429, title: "Too many requests.").ExecuteAsync(context.HttpContext);
            };
        });
    }

    private static FixedWindowRateLimiterOptions Window(int limit) => new()
    {
        PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
    };

    public static void UseApiSecurityHeaders(this WebApplication app)
    {
        if ((app.Configuration.GetSection("Security:KnownProxies").Get<string[]>() ?? []).Length > 0)
            app.UseForwardedHeaders();
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Request-ID"] = context.TraceIdentifier;
                return Task.CompletedTask;
            });
            context.Response.OnCompleted(() =>
            {
                if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method))
                    app.Logger.LogInformation("API mutation {Route} returned {Status}; actor {Actor}; trace {Trace}",
                        (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched",
                        context.Response.StatusCode, context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous", context.TraceIdentifier);
                return Task.CompletedTask;
            });
            if (!app.Environment.IsDevelopment() && !context.Request.IsHttps)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            await next(context);
        });
        if (!app.Environment.IsDevelopment()) app.UseHsts();
        app.UseExceptionHandler(handler => handler.Run(async context =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
            int status = exception is DbUpdateConcurrencyException ? 409
                : exception is BadHttpRequestException badRequest ? badRequest.StatusCode : 500;
            await Results.Problem(statusCode: status, title: status == 409
                ? "The ticket changed. Refresh and retry." : "The request could not be completed.").ExecuteAsync(context);
        }));
    }

    public static void UseApiRequestProtection(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)
                || HttpMethods.IsOptions(context.Request.Method))
            {
                await next(context);
                return;
            }
            if (context.Request.ContentLength > MaxBodyBytes)
            {
                context.Response.StatusCode = 413;
                return;
            }
            using var body = new MemoryStream();
            byte[] buffer = new byte[4096];
            int read;
            while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) > 0)
            {
                if (body.Length + read > MaxBodyBytes)
                {
                    context.Response.StatusCode = 413;
                    return;
                }
                await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
            }
            body.Position = 0;
            var original = context.Request.Body;
            context.Request.Body = body;
            try
            {
                await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
                await next(context);
            }
            catch (AntiforgeryValidationException)
            {
                await Results.Problem(statusCode: 400, title: "Invalid or missing CSRF token.").ExecuteAsync(context);
            }
            finally { context.Request.Body = original; }
        });
    }
}
