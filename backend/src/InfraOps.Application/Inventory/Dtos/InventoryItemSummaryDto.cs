namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryItemSummaryDto(
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
