namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryFormDefinitionResponse(
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    IReadOnlyCollection<InventoryFormFieldDefinitionResponse> FieldDefinitions);
