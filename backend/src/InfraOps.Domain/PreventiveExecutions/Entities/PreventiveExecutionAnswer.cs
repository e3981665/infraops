using System.Globalization;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Enums;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveExecutionAnswer
{
    private PreventiveExecutionAnswer()
    {
    }

    private PreventiveExecutionAnswer(
        Guid id,
        Guid preventiveExecutionId,
        string itemKey,
        string? value,
        string? comment)
    {
        Id = id;
        PreventiveExecutionId = preventiveExecutionId;
        ItemKey = itemKey;
        Value = value;
        Comment = comment;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveExecutionId { get; private set; }

    public string ItemKey { get; private set; } = string.Empty;

    public string? Value { get; private set; }

    public string? Comment { get; private set; }

    public static PreventiveExecutionAnswer Create(
        Guid preventiveExecutionId,
        PreventiveExecutionTemplateItem item,
        string? value,
        string? comment)
    {
        if (preventiveExecutionId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution answer execution id is required.");
        }

        var normalizedValue = NormalizeValue(item, value);
        var normalizedComment = NormalizeComment(comment);
        EnsureFailureComment(item, normalizedValue, normalizedComment);

        return new PreventiveExecutionAnswer(
            Guid.NewGuid(),
            preventiveExecutionId,
            item.ItemKey,
            normalizedValue,
            normalizedComment);
    }

    public void Update(
        PreventiveExecutionTemplateItem item,
        string? value,
        string? comment)
    {
        Value = NormalizeValue(item, value);
        Comment = NormalizeComment(comment);
        EnsureFailureComment(item, Value, Comment);
    }

    public static bool HasProvidedValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string? NormalizeValue(
        PreventiveExecutionTemplateItem item,
        string? value)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();

        return item.ItemType switch
        {
            PreventiveChecklistItemType.YesNo => NormalizeYesNoValue(item, trimmedValue),
            PreventiveChecklistItemType.Text => NormalizeTextValue(trimmedValue),
            PreventiveChecklistItemType.Numeric => NormalizeNumericValue(item, trimmedValue),
            PreventiveChecklistItemType.Select => NormalizeSelectValue(item, trimmedValue),
            _ => throw new DomainRuleException("Unsupported preventive execution answer type.")
        };
    }

    private static string NormalizeYesNoValue(
        PreventiveExecutionTemplateItem item,
        string value)
    {
        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
        {
            return "true";
        }

        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
        {
            return "false";
        }

        throw new DomainRuleException($"Answer for checklist item '{item.ItemKey}' must be yes or no.");
    }

    private static string NormalizeTextValue(string value)
    {
        if (value.Length > 2000)
        {
            throw new DomainRuleException("Preventive execution text answer cannot exceed 2000 characters.");
        }

        return value;
    }

    private static string NormalizeNumericValue(
        PreventiveExecutionTemplateItem item,
        string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var numericValue))
        {
            throw new DomainRuleException($"Answer for checklist item '{item.ItemKey}' must be numeric.");
        }

        if (item.MinimumValue.HasValue && numericValue < item.MinimumValue.Value)
        {
            throw new DomainRuleException($"Answer for checklist item '{item.ItemKey}' is below the minimum allowed value.");
        }

        if (item.MaximumValue.HasValue && numericValue > item.MaximumValue.Value)
        {
            throw new DomainRuleException($"Answer for checklist item '{item.ItemKey}' is above the maximum allowed value.");
        }

        return numericValue.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeSelectValue(
        PreventiveExecutionTemplateItem item,
        string value)
    {
        var option = item.Options.SingleOrDefault(x =>
            string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));

        if (option is null)
        {
            throw new DomainRuleException($"Answer for checklist item '{item.ItemKey}' must match one of the configured options.");
        }

        return option.Value;
    }

    private static string? NormalizeComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        var normalizedComment = comment.Trim();

        if (normalizedComment.Length > 2000)
        {
            throw new DomainRuleException("Preventive execution answer comment cannot exceed 2000 characters.");
        }

        return normalizedComment;
    }

    private static void EnsureFailureComment(
        PreventiveExecutionTemplateItem item,
        string? value,
        string? comment)
    {
        if (!item.RequiresCommentOnFailure || item.ItemType != PreventiveChecklistItemType.YesNo)
        {
            return;
        }

        if (string.Equals(value, "false", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(comment))
        {
            throw new DomainRuleException($"Checklist item '{item.ItemKey}' requires a comment when it fails.");
        }
    }
}
