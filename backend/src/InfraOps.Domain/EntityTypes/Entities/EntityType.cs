using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Domain.EntityTypes.Entities;

public sealed class EntityType
{
    private readonly List<EntityFieldDefinition> _fieldDefinitions = [];

    private EntityType()
    {
    }

    private EntityType(Guid id, string name, string code, string? description)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<EntityFieldDefinition> FieldDefinitions => _fieldDefinitions;

    public static EntityType Create(
        Guid id,
        string name,
        string code,
        string? description,
        IReadOnlyCollection<EntityFieldDefinitionDraft> fieldDefinitions)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Entity type id is required.");
        }

        var entityType = new EntityType(
            id,
            EntityTypeName.Create(name).Value,
            EntityTypeCode.Create(code).Value,
            NormalizeDescription(description));

        entityType.SyncFieldDefinitions(fieldDefinitions);

        return entityType;
    }

    public void Update(
        string name,
        string code,
        string? description,
        IReadOnlyCollection<EntityFieldDefinitionDraft> fieldDefinitions)
    {
        Name = EntityTypeName.Create(name).Value;
        Code = EntityTypeCode.Create(code).Value;
        Description = NormalizeDescription(description);

        SyncFieldDefinitions(fieldDefinitions);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SyncFieldDefinitions(IReadOnlyCollection<EntityFieldDefinitionDraft> fieldDefinitions)
    {
        fieldDefinitions ??= [];

        var normalizedKeys = fieldDefinitions
            .Select(x => EntityFieldKey.Create(x.FieldKey).Value)
            .ToArray();

        if (normalizedKeys.Length != normalizedKeys.Distinct(StringComparer.Ordinal).Count())
        {
            throw new DomainRuleException("Entity field keys must be unique within an entity type.");
        }

        var displayOrders = fieldDefinitions.Select(x => x.DisplayOrder).ToArray();

        if (displayOrders.Any(x => x <= 0))
        {
            throw new DomainRuleException("Entity field display order must be greater than zero.");
        }

        if (displayOrders.Length != displayOrders.Distinct().Count())
        {
            throw new DomainRuleException("Entity field display order must be unique within an entity type.");
        }

        var existingFields = _fieldDefinitions.ToDictionary(x => x.Id, x => x);
        var processedIds = new HashSet<Guid>();

        foreach (var fieldDraft in fieldDefinitions.OrderBy(x => x.DisplayOrder))
        {
            var normalizedKey = EntityFieldKey.Create(fieldDraft.FieldKey).Value;

            var duplicateField = _fieldDefinitions.SingleOrDefault(x =>
                x.FieldKey == normalizedKey && x.Id != fieldDraft.Id.GetValueOrDefault());

            if (duplicateField is not null)
            {
                throw new DomainRuleException("Entity field keys must be unique within an entity type.");
            }

            if (fieldDraft.Id is Guid fieldId && fieldId != Guid.Empty)
            {
                if (!existingFields.TryGetValue(fieldId, out var existingField))
                {
                    throw new DomainRuleException("Entity field definition was not found for update.");
                }

                existingField.Update(fieldDraft);
                processedIds.Add(existingField.Id);
                continue;
            }

            var newField = EntityFieldDefinition.Create(Guid.NewGuid(), Id, fieldDraft);
            _fieldDefinitions.Add(newField);
            processedIds.Add(newField.Id);
        }

        foreach (var existingField in _fieldDefinitions.Where(x => !processedIds.Contains(x.Id)))
        {
            existingField.Deactivate();
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > 500)
        {
            throw new DomainRuleException("Entity type description cannot exceed 500 characters.");
        }

        return normalizedDescription;
    }
}
