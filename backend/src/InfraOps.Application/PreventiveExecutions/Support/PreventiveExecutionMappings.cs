using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveExecutions.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Application.PreventiveExecutions.Support;

public static class PreventiveExecutionMappings
{
    public static PreventiveExecutionDetailsDto ToDetailsDto(PreventiveExecution execution)
    {
        return new PreventiveExecutionDetailsDto(
            execution.Id,
            execution.InventoryItemId,
            execution.InventoryItemDisplayName,
            execution.PreventiveTemplateId,
            execution.PreventiveTemplateName,
            execution.PreventiveTemplateCode,
            execution.EntityTypeId,
            execution.EntityTypeName,
            execution.EntityTypeCode,
            execution.RegionId,
            execution.RegionName,
            execution.SiteId,
            execution.SiteName,
            PreventiveExecutionStatusCatalog.ToValue(execution.Status),
            execution.CreatedBy,
            execution.UpdatedBy,
            execution.SubmittedBy,
            execution.CreatedAtUtc,
            execution.UpdatedAtUtc,
            execution.SubmittedAtUtc,
            execution.TemplateSections
                .OrderBy(x => x.DisplayOrder)
                .Select(ToSectionDto)
                .ToArray(),
            execution.Answers
                .OrderBy(x => x.ItemKey, StringComparer.OrdinalIgnoreCase)
                .Select(x => new PreventiveExecutionAnswerDto(x.Id, x.ItemKey, x.Value, x.Comment))
                .ToArray(),
            execution.ValidationRecords
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new PreventiveValidationRecordDto(
                    x.Id,
                    PreventiveValidationActionTypeCatalog.ToValue(x.ActionType),
                    x.ValidatorUserId,
                    x.CreatedAtUtc,
                    x.Comment))
                .ToArray());
    }

    public static PreventiveExecutionSummaryDto ToSummaryDto(PreventiveExecution execution)
    {
        return new PreventiveExecutionSummaryDto(
            execution.Id,
            execution.InventoryItemId,
            execution.InventoryItemDisplayName,
            execution.PreventiveTemplateId,
            execution.PreventiveTemplateName,
            execution.EntityTypeId,
            execution.EntityTypeName,
            execution.RegionId,
            execution.RegionName,
            execution.SiteId,
            execution.SiteName,
            PreventiveExecutionStatusCatalog.ToValue(execution.Status),
            execution.CreatedBy,
            execution.UpdatedBy,
            execution.SubmittedBy,
            execution.CreatedAtUtc,
            execution.UpdatedAtUtc,
            execution.SubmittedAtUtc);
    }

    public static PreventiveExecutionFormDefinitionDto ToFormDefinitionDto(
        InventoryItem inventoryItem,
        PreventiveTemplate preventiveTemplate)
    {
        return new PreventiveExecutionFormDefinitionDto(
            inventoryItem.Id,
            inventoryItem.DisplayName,
            inventoryItem.EntityTypeId,
            inventoryItem.EntityType?.Name ?? string.Empty,
            inventoryItem.EntityType?.Code ?? string.Empty,
            preventiveTemplate.Id,
            preventiveTemplate.Name,
            preventiveTemplate.Code,
            preventiveTemplate.Sections
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(ToTemplateSectionDto)
                .Where(x => x.ChecklistItems.Count > 0)
                .ToArray());
    }

    public static PreventiveExecutionAnswerDraft ToDraft(PreventiveExecutionAnswerInput input)
    {
        return new PreventiveExecutionAnswerDraft(input.ItemKey, input.Value, input.Comment);
    }

    private static PreventiveExecutionTemplateSectionDto ToSectionDto(PreventiveExecutionTemplateSection section)
    {
        return new PreventiveExecutionTemplateSectionDto(
            section.Id,
            section.SourceTemplateSectionId,
            section.Title,
            section.DisplayOrder,
            section.ChecklistItems
                .OrderBy(x => x.DisplayOrder)
                .Select(ToItemDto)
                .ToArray());
    }

    private static PreventiveExecutionTemplateSectionDto ToTemplateSectionDto(PreventiveTemplateSection section)
    {
        return new PreventiveExecutionTemplateSectionDto(
            section.Id,
            section.Id,
            section.Title,
            section.DisplayOrder,
            section.ChecklistItems
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(ToTemplateItemDto)
                .ToArray());
    }

    private static PreventiveExecutionTemplateItemDto ToItemDto(PreventiveExecutionTemplateItem item)
    {
        return new PreventiveExecutionTemplateItemDto(
            item.Id,
            item.SourceChecklistItemId,
            item.ItemKey,
            item.Label,
            PreventiveChecklistItemTypeCatalog.ToValue(item.ItemType),
            item.DisplayOrder,
            item.IsRequired,
            item.HelpText,
            item.IsCritical,
            item.RequiresCommentOnFailure,
            item.RequiresPhotoOnFailure,
            item.MinimumValue,
            item.MaximumValue,
            item.Options
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PreventiveExecutionOptionDto(x.Id, x.Value, x.Label, x.DisplayOrder))
                .ToArray());
    }

    private static PreventiveExecutionTemplateItemDto ToTemplateItemDto(PreventiveChecklistItem item)
    {
        return new PreventiveExecutionTemplateItemDto(
            item.Id,
            item.Id,
            item.ItemKey,
            item.Label,
            PreventiveChecklistItemTypeCatalog.ToValue(item.ItemType),
            item.DisplayOrder,
            item.IsRequired,
            item.HelpText,
            item.IsCritical,
            item.RequiresCommentOnFailure,
            item.RequiresPhotoOnFailure,
            item.MinimumValue,
            item.MaximumValue,
            item.Options
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PreventiveExecutionOptionDto(x.Id, x.Value, x.Label, x.DisplayOrder))
                .ToArray());
    }
}
