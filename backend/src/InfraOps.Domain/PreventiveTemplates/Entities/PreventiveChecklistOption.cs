using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.PreventiveTemplates.Entities;

public sealed class PreventiveChecklistOption
{
    private PreventiveChecklistOption()
    {
    }

    private PreventiveChecklistOption(
        Guid id,
        Guid checklistItemId,
        string value,
        string label,
        int displayOrder)
    {
        Id = id;
        PreventiveChecklistItemId = checklistItemId;
        Value = value;
        Label = label;
        DisplayOrder = displayOrder;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveChecklistItemId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public static PreventiveChecklistOption Create(
        Guid id,
        Guid checklistItemId,
        string value,
        string label,
        int displayOrder)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Preventive checklist option id is required.");
        }

        if (checklistItemId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive checklist option item id is required.");
        }

        return new PreventiveChecklistOption(
            id,
            checklistItemId,
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

    private static string NormalizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Preventive checklist option value is required.");
        }

        var normalizedValue = value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');

        while (normalizedValue.Contains("--", StringComparison.Ordinal))
        {
            normalizedValue = normalizedValue.Replace("--", "-", StringComparison.Ordinal);
        }

        normalizedValue = normalizedValue.Trim('-');

        if (normalizedValue.Length is 0 or > 80)
        {
            throw new DomainRuleException("Preventive checklist option value must be between 1 and 80 characters.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedValue, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new DomainRuleException("Preventive checklist option value must use lowercase letters, numbers, and hyphens only.");
        }

        return normalizedValue;
    }

    private static string NormalizeLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainRuleException("Preventive checklist option label is required.");
        }

        var normalizedLabel = label.Trim();

        if (normalizedLabel.Length > 120)
        {
            throw new DomainRuleException("Preventive checklist option label cannot exceed 120 characters.");
        }

        return normalizedLabel;
    }

    private static int NormalizeDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new DomainRuleException("Preventive checklist option display order must be greater than zero.");
        }

        return displayOrder;
    }
}
