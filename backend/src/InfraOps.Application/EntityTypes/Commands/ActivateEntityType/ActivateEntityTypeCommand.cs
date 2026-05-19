using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.EntityTypes.Commands.ActivateEntityType;

public sealed record ActivateEntityTypeCommand(Guid EntityTypeId) : ICommand;
