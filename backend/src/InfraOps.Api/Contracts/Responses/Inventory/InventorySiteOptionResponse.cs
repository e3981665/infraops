namespace InfraOps.Api.Contracts.Responses.Inventory;

public sealed record InventorySiteOptionResponse(
    Guid Id,
    Guid RegionId,
    string Code,
    string Name);
