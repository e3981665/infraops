namespace InfraOps.Api.Contracts.Responses.EntityTypes;

public sealed record EntityTypeSummaryResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    int FieldCount);
