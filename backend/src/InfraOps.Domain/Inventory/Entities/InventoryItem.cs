using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Domain.Inventory.Entities;

public sealed class InventoryItem
{
    private readonly List<InventoryAttributeValue> _attributeValues = [];

    private InventoryItem()
    {
    }

    private InventoryItem(
        Guid id,
        Guid entityTypeId,
        Guid regionId,
        Guid siteId,
        string displayName,
        InventoryStatus status,
        DateOnly? installationDate,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        EntityTypeId = entityTypeId;
        RegionId = regionId;
        SiteId = siteId;
        DisplayName = displayName;
        Status = status;
        InstallationDate = installationDate;
        IsActive = true;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid EntityTypeId { get; private set; }

    public Guid RegionId { get; private set; }

    public Guid SiteId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public InventoryStatus Status { get; private set; }

    public DateOnly? InstallationDate { get; private set; }

    public bool IsActive { get; private set; }

    public Guid CreatedBy { get; private set; }

    public Guid UpdatedBy { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public EntityType? EntityType { get; private set; }

    public Region? Region { get; private set; }

    public Site? Site { get; private set; }

    public IReadOnlyCollection<InventoryAttributeValue> AttributeValues => _attributeValues;

    public static InventoryItem Create(
        Guid id,
        EntityType entityType,
        Region region,
        Site site,
        string displayName,
        InventoryStatus status,
        DateOnly? installationDate,
        Guid createdBy,
        DateTimeOffset createdAtUtc,
        IReadOnlyCollection<InventoryAttributeValueDraft> attributeValues)
    {
        EnsureActiveSupportingRecords(entityType, region, site);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Inventory item id is required.");
        }

        if (createdBy == Guid.Empty)
        {
            throw new DomainRuleException("Inventory item creator is required.");
        }

        var inventoryItem = new InventoryItem(
            id,
            entityType.Id,
            region.Id,
            site.Id,
            NormalizeDisplayName(displayName),
            status,
            installationDate,
            createdBy,
            createdAtUtc);

        inventoryItem.SyncAttributeValues(entityType, attributeValues);

        return inventoryItem;
    }

    public void Update(
        EntityType entityType,
        Region region,
        Site site,
        string displayName,
        InventoryStatus status,
        DateOnly? installationDate,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc,
        IReadOnlyCollection<InventoryAttributeValueDraft> attributeValues)
    {
        EnsureActiveSupportingRecords(entityType, region, site);

        if (entityType.Id != EntityTypeId)
        {
            throw new DomainRuleException("Inventory item entity type cannot be changed after creation.");
        }

        if (updatedBy == Guid.Empty)
        {
            throw new DomainRuleException("Inventory item updater is required.");
        }

        RegionId = region.Id;
        SiteId = site.Id;
        DisplayName = NormalizeDisplayName(displayName);
        Status = status;
        InstallationDate = installationDate;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc;

        SyncAttributeValues(entityType, attributeValues);
    }

    public void Activate(Guid updatedBy, DateTimeOffset updatedAtUtc)
    {
        EnsureUpdatedBy(updatedBy);
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(Guid updatedBy, DateTimeOffset updatedAtUtc)
    {
        EnsureUpdatedBy(updatedBy);
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc;
    }

    private void SyncAttributeValues(
        EntityType entityType,
        IReadOnlyCollection<InventoryAttributeValueDraft> attributeValues)
    {
        attributeValues ??= [];

        var activeFieldDefinitions = entityType.FieldDefinitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToArray();

        var fieldDefinitionsByKey = activeFieldDefinitions.ToDictionary(
            x => x.FieldKey,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        var duplicateFieldKey = attributeValues
            .GroupBy(x => x.FieldKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateFieldKey is not null)
        {
            throw new DomainRuleException("Inventory attribute values must use unique field keys.");
        }

        foreach (var attributeValue in attributeValues)
        {
            if (!fieldDefinitionsByKey.ContainsKey(attributeValue.FieldKey))
            {
                throw new DomainRuleException($"Field '{attributeValue.FieldKey}' is not defined for the selected entity type.");
            }
        }

        foreach (var requiredFieldDefinition in activeFieldDefinitions.Where(x => x.IsRequired))
        {
            var submittedValue = attributeValues.SingleOrDefault(x =>
                string.Equals(x.FieldKey, requiredFieldDefinition.FieldKey, StringComparison.OrdinalIgnoreCase));

            if (submittedValue is null || !InventoryAttributeValue.HasProvidedValue(requiredFieldDefinition, submittedValue.Value))
            {
                throw new DomainRuleException($"Field '{requiredFieldDefinition.FieldKey}' is required for the selected entity type.");
            }
        }

        var existingValuesByFieldKey = _attributeValues.ToDictionary(
            x => x.FieldKey,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        var processedFieldKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var attributeValueDraft in attributeValues)
        {
            var fieldDefinition = fieldDefinitionsByKey[attributeValueDraft.FieldKey];

            if (!InventoryAttributeValue.HasProvidedValue(fieldDefinition, attributeValueDraft.Value))
            {
                continue;
            }

            if (existingValuesByFieldKey.TryGetValue(fieldDefinition.FieldKey, out var existingValue))
            {
                existingValue.Update(fieldDefinition, attributeValueDraft.Value!);
            }
            else
            {
                _attributeValues.Add(InventoryAttributeValue.Create(
                    Guid.NewGuid(),
                    Id,
                    fieldDefinition,
                    attributeValueDraft.Value!));
            }

            processedFieldKeys.Add(fieldDefinition.FieldKey);
        }

        _attributeValues.RemoveAll(x => !processedFieldKeys.Contains(x.FieldKey));
    }

    private static void EnsureActiveSupportingRecords(EntityType entityType, Region region, Site site)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(site);

        if (!entityType.IsActive)
        {
            throw new DomainRuleException("Inventory items must belong to an active entity type.");
        }

        if (!region.IsActive)
        {
            throw new DomainRuleException("Inventory items must belong to an active region.");
        }

        if (!site.IsActive)
        {
            throw new DomainRuleException("Inventory items must belong to an active site.");
        }

        if (site.RegionId != region.Id)
        {
            throw new DomainRuleException("Selected site must belong to the selected region.");
        }
    }

    private static string NormalizeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainRuleException("Inventory item display name is required.");
        }

        var normalizedDisplayName = displayName.Trim();

        if (normalizedDisplayName.Length > 200)
        {
            throw new DomainRuleException("Inventory item display name cannot exceed 200 characters.");
        }

        return normalizedDisplayName;
    }

    private static void EnsureUpdatedBy(Guid updatedBy)
    {
        if (updatedBy == Guid.Empty)
        {
            throw new DomainRuleException("Inventory item updater is required.");
        }
    }
}
