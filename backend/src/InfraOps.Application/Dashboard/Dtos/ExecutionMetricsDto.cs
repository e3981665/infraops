namespace InfraOps.Application.Dashboard.Dtos;

public sealed record ExecutionMetricsDto(
    int TotalExecutions,
    int DraftExecutions,
    int SubmittedExecutions,
    int ApprovedExecutions,
    int RejectedExecutions,
    int ReworkRequestedExecutions,
    IReadOnlyCollection<DashboardMetricPointDto> ExecutionsByEntityType);
