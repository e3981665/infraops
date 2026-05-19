using InfraOps.Domain.PreventiveTemplates.Enums;

namespace InfraOps.Domain.PreventiveTemplates.Models;

public sealed record PreventiveChecklistItemDraft(
    Guid? Id,
    string ItemKey,
    string Label,
    PreventiveChecklistItemType ItemType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? HelpText,
    bool IsCritical,
    bool RequiresCommentOnFailure,
    bool RequiresPhotoOnFailure,
    decimal? MinimumValue,
    decimal? MaximumValue,
    IReadOnlyCollection<PreventiveChecklistOptionDraft> Options);
