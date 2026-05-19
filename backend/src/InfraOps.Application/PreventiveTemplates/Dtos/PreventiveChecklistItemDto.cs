namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveChecklistItemDto(
    Guid Id,
    string ItemKey,
    string Label,
    string ItemType,
    int DisplayOrder,
    bool IsRequired,
    bool IsActive,
    string? HelpText,
    bool IsCritical,
    bool RequiresCommentOnFailure,
    bool RequiresPhotoOnFailure,
    decimal? MinimumValue,
    decimal? MaximumValue,
    IReadOnlyCollection<PreventiveChecklistOptionDto> Options);
