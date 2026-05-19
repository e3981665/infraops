namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveTemplateSectionDto(
    Guid Id,
    string Title,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<PreventiveChecklistItemDto> ChecklistItems);
