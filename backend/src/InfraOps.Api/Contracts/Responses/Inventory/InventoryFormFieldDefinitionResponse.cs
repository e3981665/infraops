namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryFormFieldDefinitionResponse(
    Guid Id,
    string FieldKey,
    string DisplayLabel,
    string FieldType,
    int DisplayOrder,
    bool IsRequired,
    string? Placeholder,
    string? HelpText,
    IReadOnlyCollection<InventoryFormFieldOptionResponse> Options);
