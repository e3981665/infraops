using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.Tests.EntityTypes.ValueObjects;

public sealed class EntityFieldKeyTests
{
    [Fact]
    public void Should_CreateEntityFieldKey_When_ValueIsLowerCamelAlphaNumeric()
    {
        var result = EntityFieldKey.Create("serialNumber1");

        Assert.Equal("serialNumber1", result.Value);
    }

    [Fact]
    public void Should_RejectEntityFieldKey_When_ValueStartsWithUppercaseLetter()
    {
        var action = () => EntityFieldKey.Create("SerialNumber");

        var exception = Assert.Throws<DomainRuleException>(action);
        Assert.Equal("Entity field key must start with a lowercase letter and use only letters and numbers.", exception.Message);
    }
}
