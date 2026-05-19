namespace InfraOps.Application.Dashboard.Dtos;

public sealed record DashboardChartsDto(
    IReadOnlyCollection<DashboardMetricPointDto> ExecutionsByMonth,
    IReadOnlyCollection<DashboardMetricPointDto> ValidationResultsByStatus,
    IReadOnlyCollection<DashboardMetricPointDto> InventoryByEntityType,
    IReadOnlyCollection<DashboardMetricPointDto> ExecutionsByEntityType);
