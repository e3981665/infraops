using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed record DeactivateInventoryItemCommand(Guid InventoryItemId) : ICommand;
