using System.Net;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Security;

public sealed class RateLimitingTests : IClassFixture<RateLimitedInfraOpsApiFactory>
{
    private readonly RateLimitedInfraOpsApiFactory _factory;

    public RateLimitingTests(RateLimitedInfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RateLimitLogin_When_AuthSensitiveLimitIsExceeded()
    {
        using var client = _factory.CreateClient();
        var request = new LoginRequest("admin@infraops.local", "wrong-password");

        using var firstResponse = await client.PostAsJsonAsync("/api/auth/login", request);
        using var secondResponse = await client.PostAsJsonAsync("/api/auth/login", request);
        using var thirdResponse = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, thirdResponse.StatusCode);
    }
}
