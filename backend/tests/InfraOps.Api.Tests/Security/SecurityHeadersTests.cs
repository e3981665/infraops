using System.Net;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Security;

public sealed class SecurityHeadersTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public SecurityHeadersTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_AddSecurityHeaders_When_ResponseIsReturned()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("camera=(), microphone=(), geolocation=()", response.Headers.GetValues("Permissions-Policy").Single());
        Assert.Equal(
            "default-src 'none'; frame-ancestors 'none'; base-uri 'none'",
            response.Headers.GetValues("Content-Security-Policy").Single());
    }
}
