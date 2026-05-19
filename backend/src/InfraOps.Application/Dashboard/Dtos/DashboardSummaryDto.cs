namespace InfraOps.Application.Dashboard.Dtos;

public sealed record DashboardSummaryDto(
    int TotalInventoryItems,
    int ActiveInventoryItems,
    int PreventiveExecutionsThisMonth,
    int PendingValidationExecutions,
    int ApprovedPreventiveExecutions,
    int RejectedPreventiveExecutions,
    int ReworkRequestedPreventiveExecutions,
    int ActiveEntityTypes,
    int ActivePreventiveTemplates);
