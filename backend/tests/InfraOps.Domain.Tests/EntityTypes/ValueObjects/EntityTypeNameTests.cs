using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.Tests.EntityTypes.ValueObjects;

public sealed class EntityTypeNameTests
{
    [Fact]
    public void Should_CreateEntityTypeName_When_ValueIsProvided()
    {
        var result = EntityTypeName.Create("  Generator  ");

        Assert.Equal("Generator", result.Value);
    }
}
