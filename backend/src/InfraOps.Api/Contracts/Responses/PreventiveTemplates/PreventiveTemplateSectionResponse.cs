namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveTemplateSectionResponse(
    Guid Id,
    string Title,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<PreventiveChecklistItemResponse> ChecklistItems);
