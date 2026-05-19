using InfraOps.Application.Inventory.Support;
using InfraOps.Domain.Inventory.Entities;

namespace InfraOps.Application.Inventory.Abstractions;

public interface IInventoryItemRepository
{
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken);

    Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InventoryItem>> ListAsync(InventoryListFilter filter, CancellationToken cancellationToken);
}
