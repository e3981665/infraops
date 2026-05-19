namespace InfraOps.Application.Dashboard.Dtos;

public sealed record ValidationMetricsDto(
    int PendingValidation,
    int Approved,
    int Rejected,
    int ReworkRequested,
    IReadOnlyCollection<DashboardMetricPointDto> ResultsByStatus);
