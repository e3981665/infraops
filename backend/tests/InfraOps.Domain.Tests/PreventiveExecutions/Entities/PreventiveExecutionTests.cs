using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveExecutions.Enums;
using InfraOps.Domain.PreventiveExecutions.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Domain.Tests.PreventiveExecutions.Entities;

public sealed class PreventiveExecutionTests
{
    [Fact]
    public void Should_StartExecution_When_InventoryItemAndActiveTemplateAreValid()
    {
        var execution = CreateExecution();

        Assert.Equal(PreventiveExecutionStatus.Draft, execution.Status);
        Assert.Equal("Quarterly UPS Inspection", execution.PreventiveTemplateName);
        Assert.Equal("UPS-01", execution.InventoryItemDisplayName);
        Assert.Equal(2, execution.TemplateSections.Count);
        Assert.Equal(5, execution.TemplateSections.SelectMany(x => x.ChecklistItems).Count());
    }

    [Fact]
    public void Should_RejectExecutionStart_When_NoActiveTemplateExists()
    {
        var template = CreateTemplate();
        template.Deactivate();

        var action = () => PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem(),
            template,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<DomainRuleException>(action);

        Assert.Equal("Preventive execution requires an active preventive template.", exception.Message);
    }

    [Fact]
    public void Should_AllowIncompleteDraftSave()
    {
        var execution = CreateExecution();

        execution.UpdateDraft(
            [new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Single(execution.Answers);
    }

    [Fact]
    public void Should_RejectSubmission_When_RequiredAnswersAreMissing()
    {
        var execution = CreateExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.Submit(
            [new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Checklist item 'activeAlarm' is required before submission.", exception.Message);
    }

    [Fact]
    public void Should_RejectInvalidSelectAnswers()
    {
        var execution = CreateExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.UpdateDraft(
            [new PreventiveExecutionAnswerDraft("batteryCondition", "unknown", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Answer for checklist item 'batteryCondition' must match one of the configured options.", exception.Message);
    }

    [Fact]
    public void Should_RejectNumericAnswersOutsideMinMax_When_Enforced()
    {
        var execution = CreateExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.UpdateDraft(
            [new PreventiveExecutionAnswerDraft("inputVoltage", "190", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Answer for checklist item 'inputVoltage' is below the minimum allowed value.", exception.Message);
    }

    [Fact]
    public void Should_RequireCommentOnFailure_When_Configured()
    {
        var execution = CreateExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.UpdateDraft(
            [new PreventiveExecutionAnswerDraft("activeAlarm", "no", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Checklist item 'activeAlarm' requires a comment when it fails.", exception.Message);
    }

    [Fact]
    public void Should_BlockDraftModificationAfterSubmission()
    {
        var execution = CreateExecution();

        execution.Submit(CreateValidAnswers(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        var exception = Assert.Throws<DomainRuleException>(() => execution.UpdateDraft(
            [new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null)],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Only draft preventive executions can be modified.", exception.Message);
    }

    [Fact]
    public void Should_ApproveSubmittedExecution()
    {
        var validatorUserId = Guid.NewGuid();
        var validatedAtUtc = DateTimeOffset.UtcNow;
        var execution = CreateSubmittedExecution();

        execution.Approve(validatorUserId, validatedAtUtc, null);

        Assert.Equal(PreventiveExecutionStatus.Approved, execution.Status);
        var record = Assert.Single(execution.ValidationRecords);
        Assert.Equal(PreventiveValidationActionType.Approved, record.ActionType);
        Assert.Equal(validatorUserId, record.ValidatorUserId);
        Assert.Equal(validatedAtUtc, record.CreatedAtUtc);
    }

    [Fact]
    public void Should_RejectSubmittedExecutionWithReason()
    {
        var execution = CreateSubmittedExecution();

        execution.Reject(Guid.NewGuid(), DateTimeOffset.UtcNow, "Voltage values are not acceptable.");

        Assert.Equal(PreventiveExecutionStatus.Rejected, execution.Status);
        var record = Assert.Single(execution.ValidationRecords);
        Assert.Equal(PreventiveValidationActionType.Rejected, record.ActionType);
        Assert.Equal("Voltage values are not acceptable.", record.Comment);
    }

    [Fact]
    public void Should_RequestReworkWithReason()
    {
        var execution = CreateSubmittedExecution();

        execution.RequestRework(Guid.NewGuid(), DateTimeOffset.UtcNow, "Attach missing alarm details.");

        Assert.Equal(PreventiveExecutionStatus.ReworkRequested, execution.Status);
        var record = Assert.Single(execution.ValidationRecords);
        Assert.Equal(PreventiveValidationActionType.ReworkRequested, record.ActionType);
        Assert.Equal("Attach missing alarm details.", record.Comment);
    }

    [Fact]
    public void Should_BlockApprovalForDraftExecution()
    {
        var execution = CreateExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.Approve(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null));

        Assert.Equal("Only submitted preventive executions can be validated.", exception.Message);
    }

    [Fact]
    public void Should_BlockRepeatedApproval()
    {
        var execution = CreateSubmittedExecution();
        execution.Approve(Guid.NewGuid(), DateTimeOffset.UtcNow, null);

        var exception = Assert.Throws<DomainRuleException>(() => execution.Approve(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null));

        Assert.Equal("Only submitted preventive executions can be validated.", exception.Message);
    }

    [Fact]
    public void Should_RequireRejectionReason()
    {
        var execution = CreateSubmittedExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.Reject(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            " "));

        Assert.Equal("Preventive rejection reason is required.", exception.Message);
    }

    [Fact]
    public void Should_RequireReworkReason()
    {
        var execution = CreateSubmittedExecution();

        var exception = Assert.Throws<DomainRuleException>(() => execution.RequestRework(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            " "));

        Assert.Equal("Preventive rework reason is required.", exception.Message);
    }

    [Fact]
    public void Should_PreserveTemplateSnapshot_When_TemplateChangesAfterExecutionStarts()
    {
        var template = CreateTemplate();
        var execution = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem(),
            template,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        template.Update(
            CreateEntityType(),
            "Changed Template",
            "changed-template",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Changed Section",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(
                            null,
                            "changedItem",
                            "Changed item",
                            PreventiveChecklistItemType.Text,
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

        Assert.Equal("Quarterly UPS Inspection", execution.PreventiveTemplateName);
        Assert.Contains(execution.TemplateSections.SelectMany(x => x.ChecklistItems), x => x.ItemKey == "equipmentClean");
        Assert.DoesNotContain(execution.TemplateSections.SelectMany(x => x.ChecklistItems), x => x.ItemKey == "changedItem");
    }

    private static IReadOnlyCollection<PreventiveExecutionAnswerDraft> CreateValidAnswers()
    {
        return
        [
            new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null),
            new PreventiveExecutionAnswerDraft("activeAlarm", "yes", null),
            new PreventiveExecutionAnswerDraft("inputVoltage", "220", null),
            new PreventiveExecutionAnswerDraft("outputVoltage", "219", null),
            new PreventiveExecutionAnswerDraft("batteryCondition", "warning", null)
        ];
    }

    private static PreventiveExecution CreateExecution()
    {
        return PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem(),
            CreateTemplate(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }

    private static PreventiveExecution CreateSubmittedExecution()
    {
        var execution = CreateExecution();
        execution.Submit(CreateValidAnswers(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        return execution;
    }

    private static InventoryItem CreateInventoryItem()
    {
        var region = new Region(Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"), "north-region", "North Region");
        var site = new Site(Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"), region.Id, "north-hub", "North Hub");

        return InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            "UPS-01",
            InventoryStatus.Operational,
            new DateOnly(2024, 4, 1),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [new InventoryAttributeValueDraft("serialNumber", "UPS-0001")]);
    }

    private static PreventiveTemplate CreateTemplate()
    {
        return PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Quarterly UPS Inspection",
            "quarterly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(null, "equipmentClean", "Equipment clean?", PreventiveChecklistItemType.YesNo, 1, true, true, null, false, false, false, null, null, []),
                        new PreventiveChecklistItemDraft(null, "activeAlarm", "Any active alarm?", PreventiveChecklistItemType.YesNo, 2, true, true, null, true, true, false, null, null, [])
                    ]),
                new PreventiveTemplateSectionDraft(
                    null,
                    "Electrical Measurements",
                    2,
                    true,
                    [
                        new PreventiveChecklistItemDraft(null, "inputVoltage", "Input voltage", PreventiveChecklistItemType.Numeric, 1, true, true, null, true, false, false, 210, 240, []),
                        new PreventiveChecklistItemDraft(null, "outputVoltage", "Output voltage", PreventiveChecklistItemType.Numeric, 2, true, true, null, true, false, false, 210, 240, []),
                        new PreventiveChecklistItemDraft(
                            null,
                            "batteryCondition",
                            "Battery condition",
                            PreventiveChecklistItemType.Select,
                            3,
                            true,
                            true,
                            null,
                            true,
                            false,
                            false,
                            null,
                            null,
                            [
                                new PreventiveChecklistOptionDraft(null, "good", "Good", 1),
                                new PreventiveChecklistOptionDraft(null, "warning", "Warning", 2),
                                new PreventiveChecklistOptionDraft(null, "critical", "Critical", 3)
                            ])
                    ])
            ]);
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups",
            null,
            [new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, [])]);
    }
}
