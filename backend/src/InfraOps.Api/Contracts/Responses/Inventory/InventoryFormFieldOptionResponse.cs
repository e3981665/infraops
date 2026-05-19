namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryFormFieldOptionResponse(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
