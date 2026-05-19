namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryAttributeValueDto(
    Guid EntityFieldDefinitionId,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    string Value);
