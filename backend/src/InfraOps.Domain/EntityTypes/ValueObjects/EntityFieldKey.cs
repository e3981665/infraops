using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.Common.Text;

namespace InfraOps.Domain.EntityTypes.ValueObjects;

public sealed record EntityFieldKey
{
    public string Value { get; }

    private EntityFieldKey(string value)
    {
        Value = value;
    }

    public static EntityFieldKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Entity field key is required.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > 80)
        {
            throw new DomainRuleException("Entity field key cannot exceed 80 characters.");
        }

        if (!IdentifierText.IsLowerCamelAlphaNumericKey(normalizedValue))
        {
            throw new DomainRuleException("Entity field key must start with a lowercase letter and use only letters and numbers.");
        }

        return new EntityFieldKey(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
