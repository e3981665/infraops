using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;

namespace InfraOps.Application.Inventory.Queries.GetInventoryItemById;

public sealed class GetInventoryItemByIdQueryHandler : IQueryHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    public GetInventoryItemByIdQueryHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    public async Task<InventoryItemDetailsDto> Handle(
        GetInventoryItemByIdQuery query,
        CancellationToken cancellationToken)
    {
        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(query.InventoryItemId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Inventory item was not found.");

        return InventoryMappings.ToDetailsDto(inventoryItem);
    }
}
