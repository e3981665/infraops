namespace InfraOps.Api.Contracts.Responses.EntityTypes;

public sealed record EntityTypeResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<EntityFieldDefinitionResponse> FieldDefinitions);
