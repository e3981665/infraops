using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.Tests.EntityTypes.ValueObjects;

public sealed class EntityTypeCodeTests
{
    [Fact]
    public void Should_NormalizeEntityTypeCode_When_ValueUsesSpacesAndUnderscores()
    {
        var result = EntityTypeCode.Create("  Generator_Main Unit  ");

        Assert.Equal("generator-main-unit", result.Value);
    }

    [Fact]
    public void Should_RejectEntityTypeCode_When_PathologicalInputContainsInvalidCharacters()
    {
        var value = $"{new string('a', 30)}!{new string('-', 20)}";

        var action = () => EntityTypeCode.Create(value);

        var exception = Assert.Throws<DomainRuleException>(action);
        Assert.Equal("Entity type code must use lowercase letters, numbers, and hyphens only.", exception.Message);
    }
}
