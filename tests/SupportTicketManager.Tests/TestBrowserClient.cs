using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace SupportTicketManager.Tests;

// Test-only transport: messages stay in memory, never sent or logged.
public sealed class TestEmail : IAccountEmail
{
    public ConcurrentQueue<(string Email, string Link)> Messages { get; } = new();
    public bool FailDelivery { get; set; }
    public Task SendAsync(string email, string subject, string link, CancellationToken cancellationToken)
    {
        if (FailDelivery) throw new InvalidOperationException("Simulated delivery failure");
        Messages.Enqueue((email, link));
        return Task.CompletedTask;
    }
    public Dictionary<string, string> Latest(string email) => QueryHelpers.ParseQuery(
        new Uri(Messages.Last(message => message.Email == email).Link).Fragment.TrimStart('#'))
        .ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
}

// Real cookie + CSRF flow. Negative tests use CreateRawClient; no security bypass.
public sealed class TestBrowserClient(HttpClient transport, TestEmail inbox)
    : HttpClient(new CsrfClientHandler(transport))
{
    public async Task ConfirmEmailAsync(string email)
    {
        var data = inbox.Latest(email);
        using var response = await this.PostAsJsonAsync("/api/auth/confirm-email",
            new { userId = data["userId"], token = data["token"] });
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class CsrfClientHandler(HttpClient transport) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head && request.Method != HttpMethod.Options)
            {
                using var csrf = await transport.GetAsync("/api/auth/csrf", cancellationToken);
                csrf.EnsureSuccessStatusCode();
                var body = await csrf.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                request.Headers.Add(ApiSecurity.CsrfHeader, body.GetProperty("token").GetString());
            }
            using var forwarded = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content, Version = request.Version, VersionPolicy = request.VersionPolicy
            };
            foreach (var header in request.Headers)
                forwarded.Headers.TryAddWithoutValidation(header.Key, header.Value);
            return await transport.SendAsync(forwarded, cancellationToken);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) transport.Dispose();
            base.Dispose(disposing);
        }
    }
}
