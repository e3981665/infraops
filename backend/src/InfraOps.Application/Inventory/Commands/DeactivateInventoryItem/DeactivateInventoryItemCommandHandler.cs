using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;

namespace InfraOps.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed class DeactivateInventoryItemCommandHandler : ICommandHandler<DeactivateInventoryItemCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateInventoryItemCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.InventoryItemId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Inventory item was not found.");

        inventoryItem.Deactivate(currentUserId, _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
