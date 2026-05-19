namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveExecutionOptionDto(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
