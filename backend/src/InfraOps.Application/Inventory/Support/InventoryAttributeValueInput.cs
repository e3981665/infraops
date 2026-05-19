namespace InfraOps.Application.Inventory.Support;

public sealed record InventoryAttributeValueInput(
    string FieldKey,
    string? Value);
