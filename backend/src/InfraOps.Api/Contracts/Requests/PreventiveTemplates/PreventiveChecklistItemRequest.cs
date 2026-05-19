namespace InfraOps.Api.Contracts.Requests.PreventiveTemplates;

public sealed record PreventiveChecklistItemRequest(
    Guid? Id,
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
    IReadOnlyCollection<PreventiveChecklistOptionRequest> Options);
