using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Responses;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Auth;

public sealed class AuthEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public AuthEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_ReturnTokens_When_LoginRequestIsValid()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@infraops.local",
            "DemoOnly-Admin-Local"));

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
    }

    [Fact]
    public async Task Should_ReturnValidationError_When_LoginRequestIsInvalid()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "not-an-email",
            string.Empty));
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("validation_error", payload.Code);
        Assert.NotNull(payload.Errors);
        Assert.Contains(nameof(LoginRequest.Email), payload.Errors.Keys);
        Assert.Contains(nameof(LoginRequest.Password), payload.Errors.Keys);
    }

    [Fact]
    public async Task Should_NotExposeSensitiveUserFields_When_LoginRequestIsValid()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@infraops.local",
            "DemoOnly-Admin-Local"));
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("password", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("roles", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("permissions", responseBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_MeEndpointIsRequestedWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnCurrentUser_When_MeEndpointIsRequestedWithValidToken()
    {
        using var client = _factory.CreateClient();
        var tokens = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        using var response = await client.GetAsync("/api/auth/me");
        var payload = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("admin@infraops.local", payload.Email);
        Assert.Contains("Admin", payload.Roles);
        Assert.Contains("users.read", payload.Permissions);
    }

    [Fact]
    public async Task Should_ReturnNewTokens_When_RefreshTokenIsValid()
    {
        using var client = _factory.CreateClient();
        var login = await LoginAsync(client);

        using var response = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login.RefreshToken));
        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.NotEqual(login.RefreshToken, payload.RefreshToken);
    }

    [Fact]
    public async Task Should_RejectRefresh_When_RefreshTokenWasRevokedByLogout()
    {
        using var client = _factory.CreateClient();
        var login = await LoginAsync(client);

        using var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(login.RefreshToken));
        using var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login.RefreshToken));

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    private static async Task<TokenResponse> LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@infraops.local",
            "DemoOnly-Admin-Local"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }
}
