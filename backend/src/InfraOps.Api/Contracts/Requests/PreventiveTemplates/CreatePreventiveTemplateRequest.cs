namespace InfraOps.Api.Contracts.Requests.PreventiveTemplates;

public sealed record CreatePreventiveTemplateRequest(
    Guid EntityTypeId,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<PreventiveTemplateSectionRequest> Sections);
