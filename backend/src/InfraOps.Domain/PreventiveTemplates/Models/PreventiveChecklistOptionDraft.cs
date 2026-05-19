namespace InfraOps.Domain.PreventiveTemplates.Models;

public sealed record PreventiveChecklistOptionDraft(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
