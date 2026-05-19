namespace InfraOps.Api.Contracts.Responses.Dashboard;

public sealed record ValidationMetricsResponse(
    int PendingValidation,
    int Approved,
    int Rejected,
    int ReworkRequested,
    IReadOnlyCollection<DashboardMetricPointResponse> ResultsByStatus);
