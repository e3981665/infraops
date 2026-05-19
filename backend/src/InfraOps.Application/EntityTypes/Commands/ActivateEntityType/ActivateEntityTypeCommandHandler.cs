using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;

namespace InfraOps.Application.EntityTypes.Commands.ActivateEntityType;

public sealed class ActivateEntityTypeCommandHandler : ICommandHandler<ActivateEntityTypeCommand>
{
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateEntityTypeCommandHandler(IEntityTypeRepository entityTypeRepository, IUnitOfWork unitOfWork)
    {
        _entityTypeRepository = entityTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateEntityTypeCommand command, CancellationToken cancellationToken)
    {
        var entityType = await _entityTypeRepository.GetByIdAsync(command.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        entityType.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
