namespace InfraOps.Api.Contracts.Responses.Dashboard;

public sealed record DashboardSummaryResponse(
    int TotalInventoryItems,
    int ActiveInventoryItems,
    int PreventiveExecutionsThisMonth,
    int PendingValidationExecutions,
    int ApprovedPreventiveExecutions,
    int RejectedPreventiveExecutions,
    int ReworkRequestedPreventiveExecutions,
    int ActiveEntityTypes,
    int ActivePreventiveTemplates);
