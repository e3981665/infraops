namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionOptionResponse(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
