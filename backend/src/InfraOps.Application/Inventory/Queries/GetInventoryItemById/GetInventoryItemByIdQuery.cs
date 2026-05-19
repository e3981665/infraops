using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Dtos;

namespace InfraOps.Application.Inventory.Queries.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery(Guid InventoryItemId) : IQuery<InventoryItemDetailsDto>;
