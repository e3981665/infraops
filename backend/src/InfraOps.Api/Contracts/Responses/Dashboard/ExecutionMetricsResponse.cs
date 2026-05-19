namespace InfraOps.Api.Contracts.Responses.Dashboard;

public sealed record ExecutionMetricsResponse(
    int TotalExecutions,
    int DraftExecutions,
    int SubmittedExecutions,
    int ApprovedExecutions,
    int RejectedExecutions,
    int ReworkRequestedExecutions,
    IReadOnlyCollection<DashboardMetricPointResponse> ExecutionsByEntityType);
