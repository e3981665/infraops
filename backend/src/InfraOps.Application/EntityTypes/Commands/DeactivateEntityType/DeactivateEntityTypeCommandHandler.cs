using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;

namespace InfraOps.Application.EntityTypes.Commands.DeactivateEntityType;

public sealed class DeactivateEntityTypeCommandHandler : ICommandHandler<DeactivateEntityTypeCommand>
{
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEntityTypeCommandHandler(IEntityTypeRepository entityTypeRepository, IUnitOfWork unitOfWork)
    {
        _entityTypeRepository = entityTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateEntityTypeCommand command, CancellationToken cancellationToken)
    {
        var entityType = await _entityTypeRepository.GetByIdAsync(command.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        entityType.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
