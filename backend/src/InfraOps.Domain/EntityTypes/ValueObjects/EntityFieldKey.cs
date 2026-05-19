using System.Text.RegularExpressions;
using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.EntityTypes.ValueObjects;

public sealed partial record EntityFieldKey
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

        if (!EntityFieldKeyRegex().IsMatch(normalizedValue))
        {
            throw new DomainRuleException("Entity field key must start with a lowercase letter and use only letters and numbers.");
        }

        return new EntityFieldKey(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("^[a-z][A-Za-z0-9]*$", RegexOptions.CultureInvariant)]
    private static partial Regex EntityFieldKeyRegex();
}
