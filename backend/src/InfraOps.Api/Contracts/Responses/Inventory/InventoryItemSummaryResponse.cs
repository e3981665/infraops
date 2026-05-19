namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryItemSummaryResponse(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    Guid RegionId,
    string RegionName,
    Guid SiteId,
    string SiteName,
    string DisplayName,
    string Status,
    DateOnly? InstallationDate,
    bool IsActive);
