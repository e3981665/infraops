using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.Common.Text;

namespace InfraOps.Domain.EntityTypes.ValueObjects;

public sealed record EntityTypeCode
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

        if (!IdentifierText.IsLowercaseSlug(normalizedValue))
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
        return IdentifierText.NormalizeSlugSeparators(value, trimHyphens: false);
    }
}
