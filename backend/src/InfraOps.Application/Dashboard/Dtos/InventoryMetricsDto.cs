namespace InfraOps.Application.Dashboard.Dtos;

public sealed record InventoryMetricsDto(
    int TotalInventoryItems,
    int ActiveInventoryItems,
    IReadOnlyCollection<DashboardMetricPointDto> InventoryByEntityType);
