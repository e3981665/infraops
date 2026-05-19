namespace InfraOps.Application.PreventiveExecutions.Support;

public sealed record PreventiveExecutionAnswerInput(
    string ItemKey,
    string? Value,
    string? Comment);
