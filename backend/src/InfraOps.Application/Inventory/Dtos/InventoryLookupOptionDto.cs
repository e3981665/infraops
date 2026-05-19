namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryLookupOptionDto(
    Guid Id,
    string Code,
    string Name);
