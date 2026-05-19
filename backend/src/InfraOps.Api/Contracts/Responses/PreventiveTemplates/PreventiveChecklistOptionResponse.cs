namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveChecklistOptionResponse(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
