namespace InfraOps.Api.Contracts.Requests.PreventiveTemplates;

public sealed record PreventiveTemplateSectionRequest(
    Guid? Id,
    string Title,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<PreventiveChecklistItemRequest> ChecklistItems);
