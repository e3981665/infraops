namespace InfraOps.Api.Contracts.Requests.PreventiveExecutions;

public sealed record PreventiveExecutionAnswerRequest(
    string ItemKey,
    string? Value,
    string? Comment);
