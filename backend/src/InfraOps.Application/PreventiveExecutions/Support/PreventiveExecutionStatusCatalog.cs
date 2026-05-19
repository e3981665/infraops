using InfraOps.Domain.PreventiveExecutions.Enums;

namespace InfraOps.Application.PreventiveExecutions.Support;

public static class PreventiveExecutionStatusCatalog
{
    private static readonly IReadOnlyDictionary<string, PreventiveExecutionStatus> Statuses =
        new Dictionary<string, PreventiveExecutionStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["draft"] = PreventiveExecutionStatus.Draft,
            ["submitted"] = PreventiveExecutionStatus.Submitted,
            ["approved"] = PreventiveExecutionStatus.Approved,
            ["rejected"] = PreventiveExecutionStatus.Rejected,
            ["reworkRequested"] = PreventiveExecutionStatus.ReworkRequested
        };

    public static IReadOnlyCollection<string> SupportedValues => Statuses.Keys.ToArray();

    public static bool TryParse(string? value, out PreventiveExecutionStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = default;
            return false;
        }

        return Statuses.TryGetValue(value.Trim(), out status);
    }

    public static string ToValue(PreventiveExecutionStatus status)
    {
        return status switch
        {
            PreventiveExecutionStatus.Draft => "draft",
            PreventiveExecutionStatus.Submitted => "submitted",
            PreventiveExecutionStatus.Approved => "approved",
            PreventiveExecutionStatus.Rejected => "rejected",
            PreventiveExecutionStatus.ReworkRequested => "reworkRequested",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported preventive execution status.")
        };
    }
}
