namespace InfraOps.Api.Contracts.Requests.Inventory;

public sealed record UpdateInventoryItemRequest(
    Guid RegionId,
    Guid SiteId,
    string DisplayName,
    string Status,
    DateOnly? InstallationDate,
    IReadOnlyCollection<InventoryAttributeValueRequest> AttributeValues);
