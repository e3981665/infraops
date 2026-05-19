using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Domain.Tests.Inventory.Entities;

public sealed class InventoryItemTests
{
    [Fact]
    public void Should_CreateInventoryItem_When_RequestIsValid()
    {
        var entityType = CreateEntityType();
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            entityType,
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            new DateOnly(2024, 4, 1),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 4, 2, 12, 0, 0, TimeSpan.Zero),
            [
                new InventoryAttributeValueDraft("serialNumber", "UPS-0001"),
                new InventoryAttributeValueDraft("phaseType", "three-phase"),
                new InventoryAttributeValueDraft("ratedPowerKw", "12.5"),
                new InventoryAttributeValueDraft("activeAlarm", "false"),
                new InventoryAttributeValueDraft("inspectionDate", "2026-04-01"),
                new InventoryAttributeValueDraft("unitCount", "2")
            ]);

        Assert.Equal("UPS Room A", inventoryItem.DisplayName);
        Assert.Equal(6, inventoryItem.AttributeValues.Count);
        Assert.Equal("12.5", inventoryItem.AttributeValues.Single(x => x.FieldKey == "ratedPowerKw").Value);
        Assert.Equal("false", inventoryItem.AttributeValues.Single(x => x.FieldKey == "activeAlarm").Value);
    }

    [Fact]
    public void Should_RejectUnknownFieldKeys_When_FieldIsNotDefinedForEntityType()
    {
        var action = () => InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            CreateRegion(),
            CreateSite(CreateRegionId()),
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [new InventoryAttributeValueDraft("unknownField", "value")]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Field 'unknownField' is not defined for the selected entity type.", exception.Message);
    }

    [Fact]
    public void Should_RejectMissingRequiredDynamicValues_When_RequiredFieldIsNotProvided()
    {
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var action = () => InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [new InventoryAttributeValueDraft("phaseType", "single-phase")]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Field 'serialNumber' is required for the selected entity type.", exception.Message);
    }

    [Fact]
    public void Should_RejectInvalidSelectOptionValues_When_ValueIsNotConfigured()
    {
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var action = () => InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                new InventoryAttributeValueDraft("serialNumber", "UPS-0001"),
                new InventoryAttributeValueDraft("phaseType", "dual-phase")
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Field 'phaseType' requires one of the configured select options.", exception.Message);
    }

    [Theory]
    [InlineData("unitCount", "1.5", "Field 'unitCount' requires a valid whole number.")]
    [InlineData("ratedPowerKw", "abc", "Field 'ratedPowerKw' requires a valid decimal value.")]
    [InlineData("activeAlarm", "maybe", "Field 'activeAlarm' requires a valid boolean value.")]
    [InlineData("inspectionDate", "04/01/2026", "Field 'inspectionDate' requires a valid date in yyyy-MM-dd format.")]
    public void Should_RejectInvalidTypedValues_When_FieldTypeDoesNotMatchSubmittedValue(
        string fieldKey,
        string value,
        string expectedMessage)
    {
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var action = () => InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                new InventoryAttributeValueDraft("serialNumber", "UPS-0001"),
                new InventoryAttributeValueDraft(fieldKey, value)
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Should_RejectDuplicateFieldValues_When_FieldKeyIsRepeated()
    {
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var action = () => InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                new InventoryAttributeValueDraft("serialNumber", "UPS-0001"),
                new InventoryAttributeValueDraft("serialNumber", "UPS-0002")
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Inventory attribute values must use unique field keys.", exception.Message);
    }

    [Fact]
    public void Should_SupportActivateAndDeactivateTransitions_When_LifecycleChanges()
    {
        var region = CreateRegion();
        var site = CreateSite(region.Id);
        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [new InventoryAttributeValueDraft("serialNumber", "UPS-0001")]);

        inventoryItem.Deactivate(Guid.NewGuid(), DateTimeOffset.UtcNow);
        inventoryItem.Activate(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(inventoryItem.IsActive);
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.NewGuid(),
            "UPS",
            "ups",
            "Critical power backup assets.",
            [
                new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, []),
                new EntityFieldDefinitionDraft(
                    null,
                    "phaseType",
                    "Phase Type",
                    EntityFieldType.Select,
                    2,
                    false,
                    true,
                    null,
                    null,
                    [
                        new EntityFieldOptionDraft(null, "single-phase", "Single-phase", 1),
                        new EntityFieldOptionDraft(null, "three-phase", "Three-phase", 2)
                    ]),
                new EntityFieldDefinitionDraft(null, "ratedPowerKw", "Rated Power", EntityFieldType.Decimal, 3, false, true, null, null, []),
                new EntityFieldDefinitionDraft(null, "activeAlarm", "Active Alarm", EntityFieldType.Boolean, 4, false, true, null, null, []),
                new EntityFieldDefinitionDraft(null, "inspectionDate", "Inspection Date", EntityFieldType.Date, 5, false, true, null, null, []),
                new EntityFieldDefinitionDraft(null, "unitCount", "Unit Count", EntityFieldType.Number, 6, false, true, null, null, [])
            ]);
    }

    private static Region CreateRegion()
    {
        return new Region(CreateRegionId(), "north-region", "North Region");
    }

    private static Site CreateSite(Guid regionId)
    {
        return new Site(Guid.NewGuid(), regionId, "north-hub", "North Hub");
    }

    private static Guid CreateRegionId()
    {
        return Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115");
    }
}
