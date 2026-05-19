using System.Text.RegularExpressions;
using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.EntityTypes.ValueObjects;

public sealed partial record EntityTypeCode
{
    public string Value { get; }

    private EntityTypeCode(string value)
    {
        Value = value;
    }

    public static EntityTypeCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Entity type code is required.");
        }

        var normalizedValue = Normalize(value);

        if (normalizedValue.Length > 60)
        {
            throw new DomainRuleException("Entity type code cannot exceed 60 characters.");
        }

        if (!EntityTypeCodeRegex().IsMatch(normalizedValue))
        {
            throw new DomainRuleException("Entity type code must use lowercase letters, numbers, and hyphens only.");
        }

        return new EntityTypeCode(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }

    private static string Normalize(string value)
    {
        var normalizedValue = value.Trim().ToLowerInvariant();
        normalizedValue = Regex.Replace(normalizedValue, @"[\s_]+", "-", RegexOptions.CultureInvariant);
        normalizedValue = Regex.Replace(normalizedValue, @"-+", "-", RegexOptions.CultureInvariant);

        return normalizedValue;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex EntityTypeCodeRegex();
}
