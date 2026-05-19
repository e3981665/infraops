using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.EntityTypes.ValueObjects;

public sealed record EntityTypeName
{
    public string Value { get; }

    private EntityTypeName(string value)
    {
        Value = value;
    }

    public static EntityTypeName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Entity type name is required.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > 100)
        {
            throw new DomainRuleException("Entity type name cannot exceed 100 characters.");
        }

        return new EntityTypeName(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
