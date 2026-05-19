using InfraOps.Application.Inventory.Dtos;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Application.Inventory.Support;

public static class InventoryMappings
{
    public static InventoryItemDetailsDto ToDetailsDto(InventoryItem inventoryItem)
    {
        return new InventoryItemDetailsDto(
            inventoryItem.Id,
            inventoryItem.EntityTypeId,
            inventoryItem.EntityType?.Name ?? string.Empty,
            inventoryItem.EntityType?.Code ?? string.Empty,
            inventoryItem.RegionId,
            inventoryItem.Region?.Name ?? string.Empty,
            inventoryItem.SiteId,
            inventoryItem.Site?.Name ?? string.Empty,
            inventoryItem.DisplayName,
            InventoryStatusCatalog.ToValue(inventoryItem.Status),
            inventoryItem.InstallationDate,
            inventoryItem.IsActive,
            inventoryItem.CreatedBy,
            inventoryItem.UpdatedBy,
            inventoryItem.CreatedAtUtc,
            inventoryItem.UpdatedAtUtc,
            inventoryItem.AttributeValues
                .OrderBy(x => x.FieldDefinition?.DisplayOrder ?? int.MaxValue)
                .Select(ToAttributeValueDto)
                .ToArray());
    }

    public static InventoryItemSummaryDto ToSummaryDto(InventoryItem inventoryItem)
    {
        return new InventoryItemSummaryDto(
            inventoryItem.Id,
            inventoryItem.EntityTypeId,
            inventoryItem.EntityType?.Name ?? string.Empty,
            inventoryItem.RegionId,
            inventoryItem.Region?.Name ?? string.Empty,
            inventoryItem.SiteId,
            inventoryItem.Site?.Name ?? string.Empty,
            inventoryItem.DisplayName,
            InventoryStatusCatalog.ToValue(inventoryItem.Status),
            inventoryItem.InstallationDate,
            inventoryItem.IsActive);
    }

    public static InventoryFormDefinitionDto ToFormDefinitionDto(EntityType entityType)
    {
        return new InventoryFormDefinitionDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.FieldDefinitions
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(ToFormFieldDefinitionDto)
                .ToArray());
    }

    public static InventoryFormMetadataDto ToFormMetadataDto(
        IReadOnlyCollection<EntityType> entityTypes,
        IReadOnlyCollection<Region> regions,
        IReadOnlyCollection<Site> sites)
    {
        return new InventoryFormMetadataDto(
            entityTypes
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new InventoryLookupOptionDto(x.Id, x.Code, x.Name))
                .ToArray(),
            regions
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new InventoryLookupOptionDto(x.Id, x.Code, x.Name))
                .ToArray(),
            sites
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new InventorySiteOptionDto(x.Id, x.RegionId, x.Code, x.Name))
                .ToArray(),
            InventoryStatusCatalog.ToOptions());
    }

    public static InventoryAttributeValueDraft ToDraft(InventoryAttributeValueInput input)
    {
        return new InventoryAttributeValueDraft(input.FieldKey, input.Value);
    }

    private static InventoryAttributeValueDto ToAttributeValueDto(InventoryAttributeValue attributeValue)
    {
        return new InventoryAttributeValueDto(
            attributeValue.EntityFieldDefinitionId,
            attributeValue.FieldKey,
            attributeValue.FieldDefinition?.DisplayLabel ?? attributeValue.FieldKey,
            attributeValue.FieldDefinition is null
                ? "text"
                : InfraOps.Application.EntityTypes.Support.EntityFieldTypeCatalog.ToValue(attributeValue.FieldDefinition.FieldType),
            attributeValue.FieldDefinition?.DisplayOrder ?? int.MaxValue,
            attributeValue.Value);
    }

    private static InventoryFormFieldDefinitionDto ToFormFieldDefinitionDto(EntityFieldDefinition fieldDefinition)
    {
        return new InventoryFormFieldDefinitionDto(
            fieldDefinition.Id,
            fieldDefinition.FieldKey,
            fieldDefinition.DisplayLabel,
            InfraOps.Application.EntityTypes.Support.EntityFieldTypeCatalog.ToValue(fieldDefinition.FieldType),
            fieldDefinition.DisplayOrder,
            fieldDefinition.IsRequired,
            fieldDefinition.Placeholder,
            fieldDefinition.HelpText,
            fieldDefinition.Options
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new InventoryFormFieldOptionDto(x.Id, x.Value, x.Label, x.DisplayOrder))
                .ToArray());
    }
}
