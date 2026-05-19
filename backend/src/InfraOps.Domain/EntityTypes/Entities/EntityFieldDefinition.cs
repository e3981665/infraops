using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.EntityTypes.Entities;

public sealed class EntityFieldDefinition
{
    private readonly List<EntityFieldOption> _options = [];

    private EntityFieldDefinition()
    {
    }

    private EntityFieldDefinition(
        Guid id,
        Guid entityTypeId,
        string fieldKey,
        string displayLabel,
        EntityFieldType fieldType,
        int displayOrder,
        bool isRequired,
        bool isActive,
        string? placeholder,
        string? helpText)
    {
        Id = id;
        EntityTypeId = entityTypeId;
        FieldKey = fieldKey;
        DisplayLabel = displayLabel;
        FieldType = fieldType;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        IsActive = isActive;
        Placeholder = placeholder;
        HelpText = helpText;
    }

    public Guid Id { get; private set; }

    public Guid EntityTypeId { get; private set; }

    public string FieldKey { get; private set; } = string.Empty;

    public string DisplayLabel { get; private set; } = string.Empty;

    public EntityFieldType FieldType { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsRequired { get; private set; }

    public bool IsActive { get; private set; }

    public string? Placeholder { get; private set; }

    public string? HelpText { get; private set; }

    public IReadOnlyCollection<EntityFieldOption> Options => _options;

    public static EntityFieldDefinition Create(Guid id, Guid entityTypeId, EntityFieldDefinitionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Entity field definition id is required.");
        }

        if (entityTypeId == Guid.Empty)
        {
            throw new DomainRuleException("Entity field definition entity type id is required.");
        }

        var entityFieldDefinition = new EntityFieldDefinition(
            id,
            entityTypeId,
            EntityFieldKey.Create(draft.FieldKey).Value,
            NormalizeDisplayLabel(draft.DisplayLabel),
            draft.FieldType,
            NormalizeDisplayOrder(draft.DisplayOrder),
            draft.IsRequired,
            draft.IsActive,
            NormalizeOptionalText(draft.Placeholder, 200, "Entity field placeholder"),
            NormalizeOptionalText(draft.HelpText, 500, "Entity field help text"));

        entityFieldDefinition.SyncOptions(draft.Options);

        return entityFieldDefinition;
    }

    public void Update(EntityFieldDefinitionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        FieldKey = EntityFieldKey.Create(draft.FieldKey).Value;
        DisplayLabel = NormalizeDisplayLabel(draft.DisplayLabel);
        FieldType = draft.FieldType;
        DisplayOrder = NormalizeDisplayOrder(draft.DisplayOrder);
        IsRequired = draft.IsRequired;
        IsActive = draft.IsActive;
        Placeholder = NormalizeOptionalText(draft.Placeholder, 200, "Entity field placeholder");
        HelpText = NormalizeOptionalText(draft.HelpText, 500, "Entity field help text");

        SyncOptions(draft.Options);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SyncOptions(IReadOnlyCollection<EntityFieldOptionDraft> optionDrafts)
    {
        optionDrafts ??= [];

        ValidateOptionConfiguration(FieldType, optionDrafts);

        if (FieldType != EntityFieldType.Select)
        {
            _options.Clear();
            return;
        }

        var existingOptions = _options.ToDictionary(x => x.Id, x => x);
        var processedIds = new HashSet<Guid>();

        foreach (var optionDraft in optionDrafts.OrderBy(x => x.DisplayOrder))
        {
            if (optionDraft.Id is Guid optionId && optionId != Guid.Empty)
            {
                if (!existingOptions.TryGetValue(optionId, out var existingOption))
                {
                    throw new DomainRuleException("Entity field option was not found for update.");
                }

                existingOption.Update(optionDraft.Value, optionDraft.Label, optionDraft.DisplayOrder);
                processedIds.Add(existingOption.Id);
                continue;
            }

            var newOption = EntityFieldOption.Create(
                Guid.NewGuid(),
                Id,
                optionDraft.Value,
                optionDraft.Label,
                optionDraft.DisplayOrder);

            _options.Add(newOption);
            processedIds.Add(newOption.Id);
        }

        _options.RemoveAll(x => !processedIds.Contains(x.Id));
    }

    private static void ValidateOptionConfiguration(
        EntityFieldType fieldType,
        IReadOnlyCollection<EntityFieldOptionDraft> optionDrafts)
    {
        if (fieldType == EntityFieldType.Select && optionDrafts.Count == 0)
        {
            throw new DomainRuleException("Select fields must define at least one option.");
        }

        if (fieldType != EntityFieldType.Select && optionDrafts.Count > 0)
        {
            throw new DomainRuleException("Only select fields can define options.");
        }

        var normalizedValues = optionDrafts
            .Select(x => x.Value.Trim().ToLowerInvariant())
            .ToArray();

        if (normalizedValues.Length != normalizedValues.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new DomainRuleException("Entity field options must use unique values.");
        }

        var displayOrders = optionDrafts.Select(x => x.DisplayOrder).ToArray();

        if (displayOrders.Any(x => x <= 0))
        {
            throw new DomainRuleException("Entity field option display order must be greater than zero.");
        }

        if (displayOrders.Length != displayOrders.Distinct().Count())
        {
            throw new DomainRuleException("Entity field options must use unique display order values.");
        }
    }

    private static int NormalizeDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new DomainRuleException("Entity field display order must be greater than zero.");
        }

        return displayOrder;
    }

    private static string NormalizeDisplayLabel(string displayLabel)
    {
        if (string.IsNullOrWhiteSpace(displayLabel))
        {
            throw new DomainRuleException("Entity field display label is required.");
        }

        var normalizedLabel = displayLabel.Trim();

        if (normalizedLabel.Length > 120)
        {
            throw new DomainRuleException("Entity field display label cannot exceed 120 characters.");
        }

        return normalizedLabel;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > maxLength)
        {
            throw new DomainRuleException($"{fieldName} cannot exceed {maxLength} characters.");
        }

        return normalizedValue;
    }
}
