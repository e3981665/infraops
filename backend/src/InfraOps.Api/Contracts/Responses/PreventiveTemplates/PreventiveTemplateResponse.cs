namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveTemplateResponse(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<PreventiveTemplateSectionResponse> Sections);
