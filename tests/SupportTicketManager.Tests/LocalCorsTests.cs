using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace SupportTicketManager.Tests;

public class LocalCorsTests
{
    [Theory]
    [InlineData("http://localhost:5500", true)]
    [InlineData("http://127.0.0.1:5500", true)]
    [InlineData("https://example.com", false)]
    public async Task PostPreflight_AllowsOnlyConfiguredLocalOrigins(string origin, bool allowed)
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "/api/tickets");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        bool hasHeader = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values);
        Assert.Equal(allowed, hasHeader);
        if (allowed)
        {
            Assert.Equal(origin, Assert.Single(values!));
            Assert.Contains("POST", string.Join(",", response.Headers.GetValues("Access-Control-Allow-Methods")));
            Assert.Contains("content-type", string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")).ToLowerInvariant());
        }
    }

    [Theory]
    [InlineData("http://localhost:5500", true)]
    [InlineData("http://127.0.0.1:5500", true)]
    [InlineData("https://example.com", false)]
    public async Task GetInfo_AllowsOnlyConfiguredLocalOrigins(string origin, bool allowed)
    {
        // The info endpoint does not resolve the database context or touch SQL.
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/name");
        request.Headers.Add("Origin", origin);
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        bool hasHeader = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values);
        Assert.Equal(allowed, hasHeader);
        if (allowed) Assert.Equal(origin, Assert.Single(values!));
    }
}
