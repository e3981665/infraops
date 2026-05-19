using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;

namespace InfraOps.Application.Inventory.Commands.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand(
    Guid InventoryItemId,
    Guid RegionId,
    Guid SiteId,
    string DisplayName,
    string Status,
    DateOnly? InstallationDate,
    IReadOnlyCollection<InventoryAttributeValueInput> AttributeValues) : ICommand<InventoryItemDetailsDto>;
