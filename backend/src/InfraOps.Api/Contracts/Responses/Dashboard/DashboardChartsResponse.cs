namespace InfraOps.Api.Contracts.Responses.Dashboard;

public sealed record DashboardChartsResponse(
    IReadOnlyCollection<DashboardMetricPointResponse> ExecutionsByMonth,
    IReadOnlyCollection<DashboardMetricPointResponse> ValidationResultsByStatus,
    IReadOnlyCollection<DashboardMetricPointResponse> InventoryByEntityType,
    IReadOnlyCollection<DashboardMetricPointResponse> ExecutionsByEntityType);
