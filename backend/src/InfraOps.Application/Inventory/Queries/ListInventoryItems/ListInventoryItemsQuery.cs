using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Dtos;

namespace InfraOps.Application.Inventory.Queries.ListInventoryItems;

public sealed record ListInventoryItemsQuery(
    Guid? EntityTypeId,
    string? Status,
    Guid? SiteId,
    Guid? RegionId,
    bool? IsActive,
    string? Search) : IQuery<IReadOnlyCollection<InventoryItemSummaryDto>>;
