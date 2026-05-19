namespace InfraOps.Domain.PreventiveTemplates.Models;

public sealed record PreventiveTemplateSectionDraft(
    Guid? Id,
    string Title,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<PreventiveChecklistItemDraft> ChecklistItems);
