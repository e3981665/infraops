namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveValidationRecordDto(
    Guid Id,
    string ActionType,
    Guid ValidatorUserId,
    DateTimeOffset CreatedAtUtc,
    string? Comment);
