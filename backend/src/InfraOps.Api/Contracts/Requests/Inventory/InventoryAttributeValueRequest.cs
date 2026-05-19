namespace InfraOps.Api.Contracts.Requests.Inventory;

public sealed record InventoryAttributeValueRequest(
    string FieldKey,
    string? Value);
