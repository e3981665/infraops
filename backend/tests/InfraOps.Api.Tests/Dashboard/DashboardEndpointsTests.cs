using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Responses;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Api.Contracts.Responses.Dashboard;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Dashboard;

public sealed class DashboardEndpointsTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public DashboardEndpointsTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_RequireAuthentication_When_GettingDashboardSummaryWithoutToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnDashboardSummaryAndCharts_When_UserIsAuthorized()
    {
        using var client = await CreateAuthenticatedClientAsync();

        using var summaryResponse = await client.GetAsync("/api/dashboard/summary");
        using var chartsResponse = await client.GetAsync("/api/dashboard/charts");
        var summary = await summaryResponse.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        var charts = await chartsResponse.Content.ReadFromJsonAsync<DashboardChartsResponse>();

        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, chartsResponse.StatusCode);
        Assert.NotNull(summary);
        Assert.NotNull(charts);
        Assert.True(summary!.ActiveEntityTypes >= 0);
        Assert.NotNull(charts!.ValidationResultsByStatus);
    }

    [Fact]
    public async Task Should_ReturnValidationError_When_DashboardDateRangeIsInvalid()
    {
        using var client = await CreateAuthenticatedClientAsync();

        using var response = await client.GetAsync(
            "/api/dashboard/summary?fromUtc=2026-05-18T12:00:00Z&toUtc=2026-05-01T12:00:00Z");
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("validation_error", payload.Code);
        Assert.Equal("One or more validation errors occurred.", payload.Message);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var tokens = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    private static async Task<TokenResponse> LoginAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@infraops.local", "InfraOps.Admin!123"));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }
}
