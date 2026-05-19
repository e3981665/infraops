using InfraOps.Domain.Inventory.Enums;

namespace InfraOps.Application.Inventory.Support;

public sealed record InventoryListFilter(
    Guid? EntityTypeId,
    InventoryStatus? Status,
    Guid? SiteId,
    Guid? RegionId,
    bool? IsActive,
    string? Search);
