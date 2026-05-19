namespace InfraOps.Api.Contracts.Requests.EntityTypes;

public sealed record UpdateEntityTypeRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<EntityFieldDefinitionRequest> FieldDefinitions);
