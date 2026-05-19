using System.Net;
using System.Net.Http.Json;
using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Responses;
using InfraOps.Api.Middleware;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Observability;

public sealed class CorrelationIdMiddlewareTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public CorrelationIdMiddlewareTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_GenerateCorrelationId_When_RequestDoesNotProvideOne()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values));
        Assert.False(string.IsNullOrWhiteSpace(values.Single()));
    }

    [Fact]
    public async Task Should_PreserveCorrelationId_When_RequestProvidesOne()
    {
        const string correlationId = "e2e-correlation-123";
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
    }

    [Fact]
    public async Task Should_IncludeCorrelationIdInErrorResponse_When_RequestFails()
    {
        const string correlationId = "login-failure-correlation";
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest("admin@infraops.local", "wrong-password"))
        };
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        using var response = await client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
        Assert.NotNull(payload);
        Assert.Equal(correlationId, payload.CorrelationId);
    }
}
