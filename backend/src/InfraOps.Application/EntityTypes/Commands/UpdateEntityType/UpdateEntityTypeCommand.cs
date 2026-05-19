using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Support;

namespace InfraOps.Application.EntityTypes.Commands.UpdateEntityType;

public sealed record UpdateEntityTypeCommand(
    Guid EntityTypeId,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<EntityFieldDefinitionInput> FieldDefinitions) : ICommand<EntityTypeDetailsDto>;
