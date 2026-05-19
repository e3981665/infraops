using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.Inventory.Commands.ActivateInventoryItem;

public sealed record ActivateInventoryItemCommand(Guid InventoryItemId) : ICommand;
