namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionAnswerResponse(
    Guid Id,
    string ItemKey,
    string? Value,
    string? Comment);
