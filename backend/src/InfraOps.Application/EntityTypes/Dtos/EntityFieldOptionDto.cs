namespace InfraOps.Application.EntityTypes.Dtos;

public sealed record EntityFieldOptionDto(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
