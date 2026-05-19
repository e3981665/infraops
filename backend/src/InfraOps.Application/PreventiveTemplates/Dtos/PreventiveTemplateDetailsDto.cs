namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveTemplateDetailsDto(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<PreventiveTemplateSectionDto> Sections);
