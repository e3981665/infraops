namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed record PreventiveTemplateSectionInput(
    Guid? Id,
    string Title,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<PreventiveChecklistItemInput> ChecklistItems);
