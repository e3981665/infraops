using InfraOps.Application.Inventory.Dtos;
using InfraOps.Domain.Inventory.Enums;

namespace InfraOps.Application.Inventory.Support;

public static class InventoryStatusCatalog
{
    private static readonly IReadOnlyDictionary<string, InventoryStatus> Statuses =
        new Dictionary<string, InventoryStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["operational"] = InventoryStatus.Operational,
            ["standby"] = InventoryStatus.Standby,
            ["maintenance"] = InventoryStatus.Maintenance,
            ["out-of-service"] = InventoryStatus.OutOfService
        };

    public static IReadOnlyCollection<string> SupportedValues { get; } = Statuses.Keys
        .OrderBy(x => x, StringComparer.Ordinal)
        .ToArray();

    public static bool TryParse(string value, out InventoryStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = default;
            return false;
        }

        return Statuses.TryGetValue(value.Trim(), out status);
    }

    public static string ToValue(InventoryStatus status)
    {
        return status switch
        {
            InventoryStatus.Operational => "operational",
            InventoryStatus.Standby => "standby",
            InventoryStatus.Maintenance => "maintenance",
            InventoryStatus.OutOfService => "out-of-service",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported inventory status.")
        };
    }

    public static string ToLabel(InventoryStatus status)
    {
        return status switch
        {
            InventoryStatus.Operational => "Operational",
            InventoryStatus.Standby => "Standby",
            InventoryStatus.Maintenance => "Maintenance",
            InventoryStatus.OutOfService => "Out of Service",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported inventory status.")
        };
    }

    public static IReadOnlyCollection<InventoryStatusOptionDto> ToOptions()
    {
        return Enum.GetValues<InventoryStatus>()
            .Select(status => new InventoryStatusOptionDto(ToValue(status), ToLabel(status)))
            .ToArray();
    }
}
