using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Authorization;

public sealed class AuthorizationEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public AuthorizationEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RejectProtectedEndpoint_When_RequestIsUnauthenticated()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/authorization/inventory-read");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_RejectProtectedEndpoint_When_BearerTokenIsInvalid()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        using var response = await client.GetAsync("/api/authorization/inventory-read");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_AllowProtectedEndpoint_When_UserHasRequiredPermission()
    {
        using var client = _factory.CreateClient();
        using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@infraops.local",
            "DemoOnly-Admin-Local"));

        loginResponse.EnsureSuccessStatusCode();

        var tokens = (await loginResponse.Content.ReadFromJsonAsync<TokenResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await client.GetAsync("/api/authorization/inventory-read");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
