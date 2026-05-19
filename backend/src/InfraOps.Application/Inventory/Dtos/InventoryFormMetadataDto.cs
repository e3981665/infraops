namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryFormMetadataDto(
    IReadOnlyCollection<InventoryLookupOptionDto> EntityTypes,
    IReadOnlyCollection<InventoryLookupOptionDto> Regions,
    IReadOnlyCollection<InventorySiteOptionDto> Sites,
    IReadOnlyCollection<InventoryStatusOptionDto> Statuses);
