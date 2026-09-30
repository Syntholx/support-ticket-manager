using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SupportTicketManager.Tests;

public class SecurityMiddlewareTests
{
    private static WebApplicationFactory<Program> Factory() => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
    private static HttpClient Client(WebApplicationFactory<Program> factory) => factory.CreateClient(new()
        { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
    private static async Task<string> Token(HttpClient client)
    {
        using var response = await client.GetAsync("/api/auth/csrf");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/forgot-password")]
    [InlineData("/api/auth/reset-password")]
    [InlineData("/api/auth/confirm-email")]
    [InlineData("/api/auth/resend-confirmation")]
    public async Task AnonymousMutation_WithoutCsrf_IsRejectedBeforeDatabaseAccess(string path)
    {
        using var factory = Factory();
        using var client = Client(factory);
        using var response = await client.PostAsJsonAsync(path, new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Invalid or missing CSRF token.", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CookieAndHeaderPair_Required_AndTokenIsNotCached()
    {
        using var factory = Factory();
        using var first = Client(factory);
        using var second = Client(factory);
        string token = await Token(first);
        await Token(second);
        second.DefaultRequestHeaders.Add(ApiSecurity.CsrfHeader, token);
        using var mismatch = await second.PostAsJsonAsync("/api/auth/login", new { email = "", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.StatusCode);
        Assert.Equal("Invalid or missing CSRF token.", (await mismatch.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("title").GetString());
        first.DefaultRequestHeaders.Add(ApiSecurity.CsrfHeader, token);
        using var validPair = await first.PostAsJsonAsync("/api/auth/login", new { email = "", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, validPair.StatusCode);
        Assert.True((await validPair.Content.ReadFromJsonAsync<JsonElement>()).TryGetProperty("message", out _));
        using var csrf = await first.GetAsync("/api/auth/csrf");
        Assert.True(csrf.Headers.CacheControl?.NoStore);
        Assert.Equal("nosniff", Assert.Single(csrf.Headers.GetValues("X-Content-Type-Options")));
        using var fresh = Client(factory);
        using var freshResponse = await fresh.GetAsync("/api/auth/csrf");
        string cookie = Assert.Single(freshResponse.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("__Host-TSM-CSRF=", cookie);
        Assert.True(cookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
        Assert.True(cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AuthRateLimit_Returns429AndRetryAfter()
    {
        using var factory = Factory();
        using var client = Client(factory);
        for (int index = 0; index < 10; index++)
        {
            using var response = await client.PostAsJsonAsync("/api/auth/login", new { });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using var rejected = await client.PostAsJsonAsync("/api/auth/login", new { });
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OversizeBody_KnownAndUnknownLength_Returns413(bool chunked)
    {
        using var factory = Factory();
        using var client = Client(factory);
        using HttpContent content = chunked ? new UnknownLengthContent() : new StringContent(new string('x', ApiSecurity.MaxBodyBytes + 1));
        using var response = await client.PostAsync("/api/auth/login", content);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    private sealed class UnknownLengthContent : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(Encoding.UTF8.GetBytes(new string('x', ApiSecurity.MaxBodyBytes + 1))).AsTask();
    }
}
