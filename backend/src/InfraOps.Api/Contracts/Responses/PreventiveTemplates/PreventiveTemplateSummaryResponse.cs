namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveTemplateSummaryResponse(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    int SectionCount,
    int ChecklistItemCount);
