namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveChecklistItemResponse(
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
    IReadOnlyCollection<PreventiveChecklistOptionResponse> Options);
