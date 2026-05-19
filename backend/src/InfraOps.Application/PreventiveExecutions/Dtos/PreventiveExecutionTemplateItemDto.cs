namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveExecutionTemplateItemDto(
    Guid Id,
    Guid SourceChecklistItemId,
    string ItemKey,
    string Label,
    string ItemType,
    int DisplayOrder,
    bool IsRequired,
    string? HelpText,
    bool IsCritical,
    bool RequiresCommentOnFailure,
    bool RequiresPhotoOnFailure,
    decimal? MinimumValue,
    decimal? MaximumValue,
    IReadOnlyCollection<PreventiveExecutionOptionDto> Options);
