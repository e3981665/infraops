namespace InfraOps.Domain.PreventiveExecutions.Models;

public sealed record PreventiveExecutionAnswerDraft(
    string ItemKey,
    string? Value,
    string? Comment);
