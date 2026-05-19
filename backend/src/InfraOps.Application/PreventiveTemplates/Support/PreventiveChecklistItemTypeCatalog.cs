using InfraOps.Domain.PreventiveTemplates.Enums;

namespace InfraOps.Application.PreventiveTemplates.Support;

public static class PreventiveChecklistItemTypeCatalog
{
    private static readonly IReadOnlyDictionary<string, PreventiveChecklistItemType> Types =
        new Dictionary<string, PreventiveChecklistItemType>(StringComparer.OrdinalIgnoreCase)
        {
            ["yesNo"] = PreventiveChecklistItemType.YesNo,
            ["text"] = PreventiveChecklistItemType.Text,
            ["numeric"] = PreventiveChecklistItemType.Numeric,
            ["select"] = PreventiveChecklistItemType.Select
        };

    public static IReadOnlyCollection<string> SupportedValues => Types.Keys.ToArray();

    public static bool TryParse(string value, out PreventiveChecklistItemType itemType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            itemType = default;
            return false;
        }

        return Types.TryGetValue(value.Trim(), out itemType);
    }

    public static string ToValue(PreventiveChecklistItemType itemType)
    {
        return itemType switch
        {
            PreventiveChecklistItemType.YesNo => "yesNo",
            PreventiveChecklistItemType.Text => "text",
            PreventiveChecklistItemType.Numeric => "numeric",
            PreventiveChecklistItemType.Select => "select",
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, "Unsupported preventive checklist item type.")
        };
    }
}
