using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;

namespace InfraOps.Domain.Tests.EntityTypes.Entities;

public sealed class EntityTypeTests
{
    [Fact]
    public void Should_CreateEntityType_When_RequestIsValid()
    {
        var entityType = EntityType.Create(
            Guid.NewGuid(),
            "UPS",
            "UPS",
            "Critical power backup assets.",
            [CreateFieldDefinitionDraft("serialNumber", "Serial Number", EntityFieldType.Text, 1)]);

        Assert.Equal("UPS", entityType.Name);
        Assert.Equal("ups", entityType.Code);
        Assert.True(entityType.IsActive);
        Assert.Single(entityType.FieldDefinitions);
    }

    [Fact]
    public void Should_RejectDuplicateFieldKeys_When_FieldKeysRepeat()
    {
        var action = () => EntityType.Create(
            Guid.NewGuid(),
            "Generator",
            "generator",
            null,
            [
                CreateFieldDefinitionDraft("serialNumber", "Serial Number", EntityFieldType.Text, 1),
                CreateFieldDefinitionDraft("serialNumber", "Backup Serial", EntityFieldType.Text, 2)
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Entity field keys must be unique within an entity type.", exception.Message);
    }

    [Fact]
    public void Should_RequireSelectOptions_When_FieldTypeIsSelect()
    {
        var action = () => EntityType.Create(
            Guid.NewGuid(),
            "Rectifier",
            "rectifier",
            null,
            [CreateFieldDefinitionDraft("phaseType", "Phase Type", EntityFieldType.Select, 1)]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Select fields must define at least one option.", exception.Message);
    }

    [Fact]
    public void Should_RejectOptions_When_FieldTypeIsNotSelect()
    {
        var action = () => EntityType.Create(
            Guid.NewGuid(),
            "HVAC",
            "hvac",
            null,
            [
                CreateFieldDefinitionDraft(
                    "manufacturer",
                    "Manufacturer",
                    EntityFieldType.Text,
                    1,
                    [new EntityFieldOptionDraft(null, "acme", "Acme", 1)])
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Only select fields can define options.", exception.Message);
    }

    [Fact]
    public void Should_DeactivateMissingFieldDefinitions_When_EntityTypeIsUpdated()
    {
        var entityType = EntityType.Create(
            Guid.NewGuid(),
            "Inverter",
            "inverter",
            null,
            [
                CreateFieldDefinitionDraft("serialNumber", "Serial Number", EntityFieldType.Text, 1),
                CreateFieldDefinitionDraft("activeAlarm", "Active Alarm", EntityFieldType.Boolean, 2)
            ]);

        var serialNumberField = entityType.FieldDefinitions.Single(x => x.FieldKey == "serialNumber");
        var activeAlarmField = entityType.FieldDefinitions.Single(x => x.FieldKey == "activeAlarm");

        entityType.Update(
            "Inverter",
            "inverter",
            "Updated",
            [
                new EntityFieldDefinitionDraft(
                    serialNumberField.Id,
                    serialNumberField.FieldKey,
                    "Asset Serial Number",
                    EntityFieldType.Text,
                    1,
                    true,
                    true,
                    null,
                    null,
                    [])
            ]);

        Assert.True(serialNumberField.IsActive);
        Assert.False(activeAlarmField.IsActive);
    }

    [Fact]
    public void Should_SupportActivateAndDeactivateTransitions()
    {
        var entityType = EntityType.Create(Guid.NewGuid(), "UPS", "ups", null, []);

        entityType.Deactivate();
        entityType.Activate();

        Assert.True(entityType.IsActive);
    }

    private static EntityFieldDefinitionDraft CreateFieldDefinitionDraft(
        string fieldKey,
        string displayLabel,
        EntityFieldType fieldType,
        int displayOrder,
        IReadOnlyCollection<EntityFieldOptionDraft>? options = null)
    {
        return new EntityFieldDefinitionDraft(
            null,
            fieldKey,
            displayLabel,
            fieldType,
            displayOrder,
            true,
            true,
            null,
            null,
            options ?? []);
    }
}
