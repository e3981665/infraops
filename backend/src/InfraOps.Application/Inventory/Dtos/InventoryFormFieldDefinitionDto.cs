namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryFormFieldDefinitionDto(
    Guid Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<InventoryFormFieldOptionDto> Options);
