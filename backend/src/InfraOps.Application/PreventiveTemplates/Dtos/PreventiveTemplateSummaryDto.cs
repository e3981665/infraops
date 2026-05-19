namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveTemplateSummaryDto(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    int SectionCount,
    int ChecklistItemCount);
