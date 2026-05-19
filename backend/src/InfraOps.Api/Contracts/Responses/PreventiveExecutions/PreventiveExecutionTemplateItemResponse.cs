namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionTemplateItemResponse(
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
    IReadOnlyCollection<PreventiveExecutionOptionResponse> Options);
