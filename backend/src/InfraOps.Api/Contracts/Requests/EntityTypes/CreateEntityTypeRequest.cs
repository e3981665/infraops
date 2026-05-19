namespace InfraOps.Api.Contracts.Requests.EntityTypes;

public sealed record CreateEntityTypeRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<EntityFieldDefinitionRequest> FieldDefinitions);
