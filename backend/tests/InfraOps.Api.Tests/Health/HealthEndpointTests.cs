using System.Net;
using InfraOps.Api.Tests.Infrastructure;

namespace InfraOps.Api.Tests.Health;

public sealed class HealthEndpointTests : IClassFixture<InfraOpsApiFactory>
{
    private readonly InfraOpsApiFactory _factory;

    public HealthEndpointTests(InfraOpsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_ReturnHealthyStatus_When_HealthEndpointIsRequested()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
