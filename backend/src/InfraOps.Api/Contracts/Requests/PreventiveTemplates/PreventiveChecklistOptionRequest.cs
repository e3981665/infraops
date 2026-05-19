namespace InfraOps.Api.Contracts.Requests.PreventiveTemplates;

public sealed record PreventiveChecklistOptionRequest(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
