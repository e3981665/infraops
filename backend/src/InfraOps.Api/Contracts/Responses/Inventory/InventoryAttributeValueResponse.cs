namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryAttributeValueResponse(
    Guid EntityFieldDefinitionId,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    string Value);
