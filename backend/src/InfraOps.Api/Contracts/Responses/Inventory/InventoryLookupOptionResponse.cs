namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventoryLookupOptionResponse(
    Guid Id,
    string Code,
    string Name);
