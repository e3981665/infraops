using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.Common.Text;

namespace InfraOps.Domain.EntityTypes.Entities;

public sealed class EntityFieldOption
{
    private EntityFieldOption()
    {
    }

    private EntityFieldOption(
        Guid id,
        Guid entityFieldDefinitionId,
        string value,
        string label,
        int displayOrder)
    {
        Id = id;
        EntityFieldDefinitionId = entityFieldDefinitionId;
        Value = value;
        Label = label;
        DisplayOrder = displayOrder;
    }

    public Guid Id { get; private set; }

    public Guid EntityFieldDefinitionId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public static EntityFieldOption Create(
        Guid id,
        Guid entityFieldDefinitionId,
        string value,
        string label,
        int displayOrder)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Entity field option id is required.");
        }

        if (entityFieldDefinitionId == Guid.Empty)
        {
            throw new DomainRuleException("Entity field option definition id is required.");
        }

        return new EntityFieldOption(
            id,
            entityFieldDefinitionId,
            NormalizeValue(value),
            NormalizeLabel(label),
            NormalizeDisplayOrder(displayOrder));
    }

    public void Update(string value, string label, int displayOrder)
    {
        Value = NormalizeValue(value);
        Label = NormalizeLabel(label);
        DisplayOrder = NormalizeDisplayOrder(displayOrder);
    }

    private static int NormalizeDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new DomainRuleException("Entity field option display order must be greater than zero.");
        }

        return displayOrder;
    }

    private static string NormalizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Entity field option value is required.");
        }

        var normalizedValue = IdentifierText.NormalizeSlugSeparators(value, trimHyphens: false);

        if (normalizedValue.Length > 80)
        {
            throw new DomainRuleException("Entity field option value cannot exceed 80 characters.");
        }

        return normalizedValue;
    }

    private static string NormalizeLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainRuleException("Entity field option label is required.");
        }

        var normalizedLabel = label.Trim();

        if (normalizedLabel.Length > 120)
        {
            throw new DomainRuleException("Entity field option label cannot exceed 120 characters.");
        }

        return normalizedLabel;
    }
}
