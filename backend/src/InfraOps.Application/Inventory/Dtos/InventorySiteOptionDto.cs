namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventorySiteOptionDto(
    Guid Id,
    Guid RegionId,
    string Code,
    string Name);
