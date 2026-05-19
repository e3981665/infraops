namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryFormDefinitionDto(
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    IReadOnlyCollection<InventoryFormFieldDefinitionDto> FieldDefinitions);
