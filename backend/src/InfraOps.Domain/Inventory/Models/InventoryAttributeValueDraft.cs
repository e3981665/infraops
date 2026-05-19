namespace InfraOps.Domain.Inventory.Models;

public sealed record InventoryAttributeValueDraft(
    string FieldKey,
    string? Value);
