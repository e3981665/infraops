namespace InfraOps.Api.Contracts.Requests.PreventiveTemplates;

public sealed record UpdatePreventiveTemplateRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<PreventiveTemplateSectionRequest> Sections);
