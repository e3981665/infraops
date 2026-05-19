namespace InfraOps.Application.EntityTypes.Dtos;

public sealed record EntityTypeSummaryDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    int FieldCount);
