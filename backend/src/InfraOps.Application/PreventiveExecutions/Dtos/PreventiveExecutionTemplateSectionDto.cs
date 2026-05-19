namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveExecutionTemplateSectionDto(
    Guid Id,
    Guid SourceTemplateSectionId,
    string Title,
    int DisplayOrder,
    IReadOnlyCollection<PreventiveExecutionTemplateItemDto> ChecklistItems);
