namespace InfraOps.Application.Inventory.Dtos;

public sealed record InventoryFormFieldOptionDto(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
