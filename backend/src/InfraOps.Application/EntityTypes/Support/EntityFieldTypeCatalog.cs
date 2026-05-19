using InfraOps.Domain.EntityTypes.Enums;

namespace InfraOps.Application.EntityTypes.Support;

public static class EntityFieldTypeCatalog
{
    private static readonly IReadOnlyDictionary<string, EntityFieldType> Types =
        new Dictionary<string, EntityFieldType>(StringComparer.OrdinalIgnoreCase)
        {
            ["text"] = EntityFieldType.Text,
            ["textarea"] = EntityFieldType.TextArea,
            ["number"] = EntityFieldType.Number,
            ["decimal"] = EntityFieldType.Decimal,
            ["boolean"] = EntityFieldType.Boolean,
            ["date"] = EntityFieldType.Date,
            ["select"] = EntityFieldType.Select
        };

    public static IReadOnlyCollection<string> SupportedValues { get; } = Types.Keys
        .OrderBy(x => x, StringComparer.Ordinal)
        .ToArray();

    public static bool TryParse(string value, out EntityFieldType fieldType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            fieldType = default;
            return false;
        }

        return Types.TryGetValue(value.Trim(), out fieldType);
    }

    public static string ToValue(EntityFieldType fieldType)
    {
        return fieldType switch
        {
            EntityFieldType.Text => "text",
            EntityFieldType.TextArea => "textarea",
            EntityFieldType.Number => "number",
            EntityFieldType.Decimal => "decimal",
            EntityFieldType.Boolean => "boolean",
            EntityFieldType.Date => "date",
            EntityFieldType.Select => "select",
            _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, "Unsupported entity field type.")
        };
    }
}
