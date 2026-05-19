using InfraOps.Application.Diagnostics.Queries.GetApplicationInfo;

namespace InfraOps.Application.Tests.Diagnostics.Queries.GetApplicationInfo;

public sealed class GetApplicationInfoQueryHandlerTests
{
    [Fact]
    public async Task Should_ReturnApplicationMetadata_When_QueryIsHandled()
    {
        var handler = new GetApplicationInfoQueryHandler();

        var result = await handler.Handle(new GetApplicationInfoQuery(), CancellationToken.None);

        Assert.Equal("InfraOps", result.ProductName);
        Assert.Equal("Clean Architecture", result.ArchitectureStyle);
        Assert.Contains("Inventory", result.Modules);
    }
}
