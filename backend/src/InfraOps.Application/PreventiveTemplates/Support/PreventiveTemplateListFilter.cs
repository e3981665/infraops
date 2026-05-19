namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed record PreventiveTemplateListFilter(
    Guid? EntityTypeId,
    bool? IsActive,
    string? Search);
