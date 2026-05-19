namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryFormMetadataResponse(
    IReadOnlyCollection<InventoryLookupOptionResponse> EntityTypes,
    IReadOnlyCollection<InventoryLookupOptionResponse> Regions,
    IReadOnlyCollection<InventorySiteOptionResponse> Sites,
    IReadOnlyCollection<InventoryStatusOptionResponse> Statuses);
