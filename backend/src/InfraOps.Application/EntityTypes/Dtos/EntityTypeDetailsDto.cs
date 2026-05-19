namespace InfraOps.Application.EntityTypes.Dtos;

public sealed record EntityTypeDetailsDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<EntityFieldDefinitionDto> FieldDefinitions);
