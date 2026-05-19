using System.Globalization;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;

namespace InfraOps.Domain.Inventory.Entities;

public sealed class InventoryAttributeValue
{
    private InventoryAttributeValue()
    {
    }

    private InventoryAttributeValue(
        Guid id,
        Guid inventoryItemId,
        Guid entityFieldDefinitionId,
        string fieldKey,
        string value)
    {
        Id = id;
        InventoryItemId = inventoryItemId;
        EntityFieldDefinitionId = entityFieldDefinitionId;
        FieldKey = fieldKey;
        Value = value;
    }

    public Guid Id { get; private set; }

    public Guid InventoryItemId { get; private set; }

    public Guid EntityFieldDefinitionId { get; private set; }

    public string FieldKey { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public EntityFieldDefinition? FieldDefinition { get; private set; }

    public static InventoryAttributeValue Create(
        Guid id,
        Guid inventoryItemId,
        EntityFieldDefinition fieldDefinition,
        string rawValue)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinition);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Inventory attribute value id is required.");
        }

        if (inventoryItemId == Guid.Empty)
        {
            throw new DomainRuleException("Inventory item id is required for attribute values.");
        }

        return new InventoryAttributeValue(
            id,
            inventoryItemId,
            fieldDefinition.Id,
            fieldDefinition.FieldKey,
            NormalizeValue(fieldDefinition, rawValue));
    }

    public void Update(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinition);

        EntityFieldDefinitionId = fieldDefinition.Id;
        FieldKey = fieldDefinition.FieldKey;
        Value = NormalizeValue(fieldDefinition, rawValue);
    }

    public static bool HasProvidedValue(EntityFieldDefinition fieldDefinition, string? rawValue)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinition);

        if (rawValue is null)
        {
            return false;
        }

        return fieldDefinition.FieldType == EntityFieldType.Boolean
            ? !string.IsNullOrWhiteSpace(rawValue.Trim())
            : !string.IsNullOrWhiteSpace(rawValue);
    }

    public static string NormalizeValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        ArgumentNullException.ThrowIfNull(fieldDefinition);

        if (!HasProvidedValue(fieldDefinition, rawValue))
        {
            throw new DomainRuleException($"Inventory attribute value is required for field '{fieldDefinition.FieldKey}'.");
        }

        var normalizedRawValue = rawValue.Trim();

        return fieldDefinition.FieldType switch
        {
            EntityFieldType.Text => normalizedRawValue,
            EntityFieldType.TextArea => normalizedRawValue,
            EntityFieldType.Number => NormalizeNumberValue(fieldDefinition, normalizedRawValue),
            EntityFieldType.Decimal => NormalizeDecimalValue(fieldDefinition, normalizedRawValue),
            EntityFieldType.Boolean => NormalizeBooleanValue(fieldDefinition, normalizedRawValue),
            EntityFieldType.Date => NormalizeDateValue(fieldDefinition, normalizedRawValue),
            EntityFieldType.Select => NormalizeSelectValue(fieldDefinition, normalizedRawValue),
            _ => throw new DomainRuleException($"Field type '{fieldDefinition.FieldType}' is not supported.")
        };
    }

    private static string NormalizeNumberValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numberValue))
        {
            throw new DomainRuleException($"Field '{fieldDefinition.FieldKey}' requires a valid whole number.");
        }

        return numberValue.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeDecimalValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        if (!decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
        {
            throw new DomainRuleException($"Field '{fieldDefinition.FieldKey}' requires a valid decimal value.");
        }

        return decimalValue.ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeBooleanValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        if (!bool.TryParse(rawValue, out var booleanValue))
        {
            throw new DomainRuleException($"Field '{fieldDefinition.FieldKey}' requires a valid boolean value.");
        }

        return booleanValue ? "true" : "false";
    }

    private static string NormalizeDateValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        if (!DateOnly.TryParseExact(rawValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
        {
            throw new DomainRuleException($"Field '{fieldDefinition.FieldKey}' requires a valid date in yyyy-MM-dd format.");
        }

        return dateValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string NormalizeSelectValue(EntityFieldDefinition fieldDefinition, string rawValue)
    {
        var selectedOption = fieldDefinition.Options.SingleOrDefault(x =>
            string.Equals(x.Value, rawValue, StringComparison.OrdinalIgnoreCase));

        if (selectedOption is null)
        {
            throw new DomainRuleException($"Field '{fieldDefinition.FieldKey}' requires one of the configured select options.");
        }

        return selectedOption.Value;
    }
}
