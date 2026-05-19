using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.Common.Text;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Domain.PreventiveTemplates.Entities;

public sealed class PreventiveChecklistItem
{
    private readonly List<PreventiveChecklistOption> _options = [];

    private PreventiveChecklistItem()
    {
    }

    private PreventiveChecklistItem(
        Guid id,
        Guid sectionId,
        string itemKey,
        string label,
        PreventiveChecklistItemType itemType,
        int displayOrder,
        bool isRequired,
        bool isActive,
        string? helpText,
        bool isCritical,
        bool requiresCommentOnFailure,
        bool requiresPhotoOnFailure,
        decimal? minimumValue,
        decimal? maximumValue)
    {
        Id = id;
        PreventiveTemplateSectionId = sectionId;
        ItemKey = itemKey;
        Label = label;
        ItemType = itemType;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        IsActive = isActive;
        HelpText = helpText;
        IsCritical = isCritical;
        RequiresCommentOnFailure = requiresCommentOnFailure;
        RequiresPhotoOnFailure = requiresPhotoOnFailure;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveTemplateSectionId { get; private set; }

    public string ItemKey { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public PreventiveChecklistItemType ItemType { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsRequired { get; private set; }

    public bool IsActive { get; private set; }

    public string? HelpText { get; private set; }

    public bool IsCritical { get; private set; }

    public bool RequiresCommentOnFailure { get; private set; }

    public bool RequiresPhotoOnFailure { get; private set; }

    public decimal? MinimumValue { get; private set; }

    public decimal? MaximumValue { get; private set; }

    public IReadOnlyCollection<PreventiveChecklistOption> Options => _options;

    public static PreventiveChecklistItem Create(
        Guid id,
        Guid sectionId,
        PreventiveChecklistItemDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Preventive checklist item id is required.");
        }

        if (sectionId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive checklist item section id is required.");
        }

        var item = new PreventiveChecklistItem(
            id,
            sectionId,
            NormalizeItemKey(draft.ItemKey),
            NormalizeLabel(draft.Label),
            draft.ItemType,
            NormalizeDisplayOrder(draft.DisplayOrder),
            draft.IsRequired,
            draft.IsActive,
            NormalizeHelpText(draft.HelpText),
            draft.IsCritical,
            draft.RequiresCommentOnFailure,
            draft.RequiresPhotoOnFailure,
            null,
            null);

        item.ApplyTypeConfiguration(draft);

        return item;
    }

    public void Update(PreventiveChecklistItemDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        ItemKey = NormalizeItemKey(draft.ItemKey);
        Label = NormalizeLabel(draft.Label);
        ItemType = draft.ItemType;
        DisplayOrder = NormalizeDisplayOrder(draft.DisplayOrder);
        IsRequired = draft.IsRequired;
        IsActive = draft.IsActive;
        HelpText = NormalizeHelpText(draft.HelpText);
        IsCritical = draft.IsCritical;
        RequiresCommentOnFailure = draft.RequiresCommentOnFailure;
        RequiresPhotoOnFailure = draft.RequiresPhotoOnFailure;

        ApplyTypeConfiguration(draft);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static string NormalizeItemKey(string itemKey)
    {
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            throw new DomainRuleException("Preventive checklist item key is required.");
        }

        var normalizedKey = itemKey.Trim();

        if (normalizedKey.Length > 80)
        {
            throw new DomainRuleException("Preventive checklist item key cannot exceed 80 characters.");
        }

        if (!IdentifierText.IsLowerCamelAlphaNumericKey(normalizedKey))
        {
            throw new DomainRuleException("Preventive checklist item key must start with a lowercase letter and use only letters and numbers.");
        }

        return normalizedKey;
    }

    private void ApplyTypeConfiguration(PreventiveChecklistItemDraft draft)
    {
        ValidateTypeConfiguration(draft);

        if (draft.ItemType == PreventiveChecklistItemType.Numeric)
        {
            MinimumValue = draft.MinimumValue;
            MaximumValue = draft.MaximumValue;
        }
        else
        {
            MinimumValue = null;
            MaximumValue = null;
        }

        SyncOptions(draft.Options);
    }

    private void SyncOptions(IReadOnlyCollection<PreventiveChecklistOptionDraft> optionDrafts)
    {
        optionDrafts ??= [];

        if (ItemType != PreventiveChecklistItemType.Select)
        {
            _options.Clear();
            return;
        }

        var existingOptions = _options.ToDictionary(x => x.Id, x => x);
        var processedIds = new HashSet<Guid>();

        foreach (var optionDraft in optionDrafts.OrderBy(x => x.DisplayOrder))
        {
            if (optionDraft.Id is Guid optionId && optionId != Guid.Empty)
            {
                if (!existingOptions.TryGetValue(optionId, out var existingOption))
                {
                    throw new DomainRuleException("Preventive checklist option was not found for update.");
                }

                existingOption.Update(optionDraft.Value, optionDraft.Label, optionDraft.DisplayOrder);
                processedIds.Add(existingOption.Id);
                continue;
            }

            var newOption = PreventiveChecklistOption.Create(
                Guid.NewGuid(),
                Id,
                optionDraft.Value,
                optionDraft.Label,
                optionDraft.DisplayOrder);

            _options.Add(newOption);
            processedIds.Add(newOption.Id);
        }

        _options.RemoveAll(x => !processedIds.Contains(x.Id));
    }

    private static void ValidateTypeConfiguration(PreventiveChecklistItemDraft draft)
    {
        var options = draft.Options ?? [];

        if (draft.ItemType == PreventiveChecklistItemType.Select && options.Count == 0)
        {
            throw new DomainRuleException("Select checklist items must define at least one option.");
        }

        if (draft.ItemType != PreventiveChecklistItemType.Select && options.Count > 0)
        {
            throw new DomainRuleException("Only select checklist items can define options.");
        }

        if (draft.ItemType == PreventiveChecklistItemType.Numeric)
        {
            if (draft.MinimumValue.HasValue && draft.MaximumValue.HasValue && draft.MinimumValue > draft.MaximumValue)
            {
                throw new DomainRuleException("Preventive numeric checklist item minimum value cannot be greater than the maximum value.");
            }
        }
        else if (draft.MinimumValue.HasValue || draft.MaximumValue.HasValue)
        {
            throw new DomainRuleException("Only numeric checklist items can define minimum or maximum values.");
        }

        var normalizedOptionValues = options
            .Select(x => x.Value.Trim().ToLowerInvariant())
            .ToArray();

        if (normalizedOptionValues.Length != normalizedOptionValues.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new DomainRuleException("Preventive checklist item options must use unique values.");
        }

        var optionDisplayOrders = options.Select(x => x.DisplayOrder).ToArray();

        if (optionDisplayOrders.Any(x => x <= 0))
        {
            throw new DomainRuleException("Preventive checklist option display order must be greater than zero.");
        }

        if (optionDisplayOrders.Length != optionDisplayOrders.Distinct().Count())
        {
            throw new DomainRuleException("Preventive checklist item options must use unique display order values.");
        }
    }

    private static int NormalizeDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new DomainRuleException("Preventive checklist item display order must be greater than zero.");
        }

        return displayOrder;
    }

    private static string NormalizeLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainRuleException("Preventive checklist item label is required.");
        }

        var normalizedLabel = label.Trim();

        if (normalizedLabel.Length > 160)
        {
            throw new DomainRuleException("Preventive checklist item label cannot exceed 160 characters.");
        }

        return normalizedLabel;
    }

    private static string? NormalizeHelpText(string? helpText)
    {
        if (string.IsNullOrWhiteSpace(helpText))
        {
            return null;
        }

        var normalizedHelpText = helpText.Trim();

        if (normalizedHelpText.Length > 500)
        {
            throw new DomainRuleException("Preventive checklist item help text cannot exceed 500 characters.");
        }

        return normalizedHelpText;
    }
}
