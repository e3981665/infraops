namespace InfraOps.Api.Contracts.Responses.Dashboard;

public sealed record InventoryMetricsResponse(
    int TotalInventoryItems,
    int ActiveInventoryItems,
    IReadOnlyCollection<DashboardMetricPointResponse> InventoryByEntityType);
