namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed record PreventiveChecklistOptionInput(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
