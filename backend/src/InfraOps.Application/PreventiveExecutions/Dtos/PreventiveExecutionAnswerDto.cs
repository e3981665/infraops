namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveExecutionAnswerDto(
    Guid Id,
    string ItemKey,
    string? Value,
    string? Comment);
