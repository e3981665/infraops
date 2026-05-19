using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveExecutionTemplateItem
{
    private readonly List<PreventiveExecutionTemplateOption> _options = [];

    private PreventiveExecutionTemplateItem()
    {
    }

    private PreventiveExecutionTemplateItem(
        Guid id,
        Guid sectionId,
        Guid sourceChecklistItemId,
        string itemKey,
        string label,
        PreventiveChecklistItemType itemType,
        int displayOrder,
        bool isRequired,
        string? helpText,
        bool isCritical,
        bool requiresCommentOnFailure,
        bool requiresPhotoOnFailure,
        decimal? minimumValue,
        decimal? maximumValue)
    {
        Id = id;
        PreventiveExecutionTemplateSectionId = sectionId;
        SourceChecklistItemId = sourceChecklistItemId;
        ItemKey = itemKey;
        Label = label;
        ItemType = itemType;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        HelpText = helpText;
        IsCritical = isCritical;
        RequiresCommentOnFailure = requiresCommentOnFailure;
        RequiresPhotoOnFailure = requiresPhotoOnFailure;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveExecutionTemplateSectionId { get; private set; }

    public Guid SourceChecklistItemId { get; private set; }

    public string ItemKey { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public PreventiveChecklistItemType ItemType { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsRequired { get; private set; }

    public string? HelpText { get; private set; }

    public bool IsCritical { get; private set; }

    public bool RequiresCommentOnFailure { get; private set; }

    public bool RequiresPhotoOnFailure { get; private set; }

    public decimal? MinimumValue { get; private set; }

    public decimal? MaximumValue { get; private set; }

    public IReadOnlyCollection<PreventiveExecutionTemplateOption> Options => _options;

    public static PreventiveExecutionTemplateItem Create(
        Guid sectionId,
        PreventiveChecklistItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (sectionId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution template item section id is required.");
        }

        var snapshotItem = new PreventiveExecutionTemplateItem(
            Guid.NewGuid(),
            sectionId,
            item.Id,
            item.ItemKey,
            item.Label,
            item.ItemType,
            item.DisplayOrder,
            item.IsRequired,
            item.HelpText,
            item.IsCritical,
            item.RequiresCommentOnFailure,
            item.RequiresPhotoOnFailure,
            item.MinimumValue,
            item.MaximumValue);

        foreach (var option in item.Options.OrderBy(x => x.DisplayOrder))
        {
            snapshotItem._options.Add(PreventiveExecutionTemplateOption.Create(snapshotItem.Id, option));
        }

        return snapshotItem;
    }
}
