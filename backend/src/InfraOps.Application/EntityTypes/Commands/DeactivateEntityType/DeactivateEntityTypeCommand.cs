using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.EntityTypes.Commands.DeactivateEntityType;

public sealed record DeactivateEntityTypeCommand(Guid EntityTypeId) : ICommand;
