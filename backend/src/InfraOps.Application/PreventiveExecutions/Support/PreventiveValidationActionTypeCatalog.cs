using InfraOps.Domain.PreventiveExecutions.Enums;

namespace InfraOps.Application.PreventiveExecutions.Support;

public static class PreventiveValidationActionTypeCatalog
{
    public static string ToValue(PreventiveValidationActionType actionType)
    {
        return actionType switch
        {
            PreventiveValidationActionType.Approved => "approved",
            PreventiveValidationActionType.Rejected => "rejected",
            PreventiveValidationActionType.ReworkRequested => "reworkRequested",
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Unsupported preventive validation action.")
        };
    }
}
