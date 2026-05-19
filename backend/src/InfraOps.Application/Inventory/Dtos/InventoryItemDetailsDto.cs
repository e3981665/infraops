namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryItemDetailsDto(
    Guid Id,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    Guid RegionId,
    string RegionName,
    Guid SiteId,
    string SiteName,
    string DisplayName,
    string Status,
    DateOnly? InstallationDate,
    bool IsActive,
    Guid CreatedBy,
    Guid UpdatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<InventoryAttributeValueDto> AttributeValues);
