namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveValidationRecordResponse(
    Guid Id,
    string ActionType,
    Guid ValidatorUserId,
    DateTimeOffset CreatedAtUtc,
    string? Comment);
