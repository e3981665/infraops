using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Domain.Tests.PreventiveTemplates.Entities;

public sealed class PreventiveTemplateTests
{
    [Fact]
    public void Should_CreatePreventiveTemplate_When_RequestIsValid()
    {
        var template = PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            "Monthly preventive checklist.",
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "equipmentClean",
                            "Equipment clean?",
                            PreventiveChecklistItemType.YesNo,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            null,
                            null,
                            [])
                    ])
            ]);

        Assert.Equal("Monthly UPS Inspection", template.Name);
        Assert.Equal("monthly-ups-inspection", template.Code);
        Assert.Single(template.Sections);
        Assert.Single(template.Sections.Single().ChecklistItems);
    }

    [Fact]
    public void Should_RejectDuplicateItemKeysWithinTemplate_When_KeyIsRepeatedAcrossSections()
    {
        var action = () => PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "equipmentClean",
                            "Equipment clean?",
                            PreventiveChecklistItemType.YesNo,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            null,
                            null,
                            [])
                    ]),
                new PreventiveTemplateSectionDraft(
                    null,
                    "Electrical",
                    2,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "equipmentClean",
                            "Input voltage",
                            PreventiveChecklistItemType.Numeric,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            200,
                            260,
                            [])
                    ])
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Preventive checklist item keys must be unique within a template.", exception.Message);
    }

    [Fact]
    public void Should_RequireSelectOptions_When_ChecklistItemTypeIsSelect()
    {
        var action = () => PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Electrical",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "batteryCondition",
                            "Battery condition",
                            PreventiveChecklistItemType.Select,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            null,
                            null,
                            [])
                    ])
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Select checklist items must define at least one option.", exception.Message);
    }

    [Fact]
    public void Should_RejectInvalidNumericBounds_When_MinimumValueIsGreaterThanMaximumValue()
    {
        var action = () => PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Electrical",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "inputVoltage",
                            "Input voltage",
                            PreventiveChecklistItemType.Numeric,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            260,
                            200,
                            [])
                    ])
            ]);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Preventive numeric checklist item minimum value cannot be greater than the maximum value.", exception.Message);
    }

    [Fact]
    public void Should_SupportActivateAndDeactivateTransitions_When_LifecycleChanges()
    {
        var template = CreateTemplate();

        template.Deactivate();
        template.Activate();

        Assert.True(template.IsActive);
    }

    [Fact]
    public void Should_PreserveDeterministicOrdering_When_SectionsAndItemsUseDisplayOrder()
    {
        var template = PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Electrical",
                    2,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "outputVoltage",
                            "Output voltage",
                            PreventiveChecklistItemType.Numeric,
                            2,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            200,
                            260,
                            []),
                        new PreventiveChecklistItemDraft(
                            null,
                            "inputVoltage",
                            "Input voltage",
                            PreventiveChecklistItemType.Numeric,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            200,
                            260,
                            [])
                    ]),
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "equipmentClean",
                            "Equipment clean?",
                            PreventiveChecklistItemType.YesNo,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            null,
                            null,
                            [])
                    ])
            ]);

        Assert.Equal(
            ["Visual Inspection", "Electrical"],
            template.Sections.OrderBy(x => x.DisplayOrder).Select(x => x.Title).ToArray());
        Assert.Equal(
            ["inputVoltage", "outputVoltage"],
            template.Sections.Single(x => x.Title == "Electrical")
                .ChecklistItems
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.ItemKey)
                .ToArray());
    }

    private static PreventiveTemplate CreateTemplate()
    {
        return PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Monthly UPS Inspection",
            "monthly-ups-inspection",
            "Monthly preventive checklist.",
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "equipmentClean",
                            "Equipment clean?",
                            PreventiveChecklistItemType.YesNo,
                            1,
                            true,
                            true,
                            null,
                            false,
                            false,
                            false,
                            null,
                            null,
                            [])
                    ])
            ]);
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.NewGuid(),
            "UPS",
            "ups",
            "Critical power backup assets.",
            [new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, [])]);
    }
}
