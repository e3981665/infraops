using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Application.PreventiveTemplates.Support;

public static class PreventiveTemplateMappings
{
    public static PreventiveTemplateDetailsDto ToDetailsDto(
        PreventiveTemplate preventiveTemplate,
        bool activeOnly = false)
    {
        var sections = preventiveTemplate.Sections.AsEnumerable();

        if (activeOnly)
        {
            sections = sections.Where(x => x.IsActive);
        }

        return new PreventiveTemplateDetailsDto(
            preventiveTemplate.Id,
            preventiveTemplate.EntityTypeId,
            preventiveTemplate.EntityType?.Name ?? string.Empty,
            preventiveTemplate.EntityType?.Code ?? string.Empty,
            preventiveTemplate.Name,
            preventiveTemplate.Code,
            preventiveTemplate.Description,
            preventiveTemplate.IsActive,
            sections
                .OrderBy(x => x.DisplayOrder)
                .Select(section => ToSectionDto(section, activeOnly))
                .ToArray());
    }

    public static PreventiveTemplateSummaryDto ToSummaryDto(PreventiveTemplate preventiveTemplate)
    {
        return new PreventiveTemplateSummaryDto(
            preventiveTemplate.Id,
            preventiveTemplate.EntityTypeId,
            preventiveTemplate.EntityType?.Name ?? string.Empty,
            preventiveTemplate.Name,
            preventiveTemplate.Code,
            preventiveTemplate.Description,
            preventiveTemplate.IsActive,
            preventiveTemplate.Sections.Count(x => x.IsActive),
            preventiveTemplate.Sections
                .Where(x => x.IsActive)
                .SelectMany(x => x.ChecklistItems)
                .Count(x => x.IsActive));
    }

    public static PreventiveTemplateFormMetadataDto ToFormMetadataDto(
        IReadOnlyCollection<EntityType> entityTypes)
    {
        return new PreventiveTemplateFormMetadataDto(
            entityTypes
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new PreventiveTemplateEntityTypeOptionDto(x.Id, x.Code, x.Name))
                .ToArray());
    }

    public static PreventiveTemplateSectionDraft ToDraft(PreventiveTemplateSectionInput input)
    {
        return new PreventiveTemplateSectionDraft(
            input.Id,
            input.Title,
            input.DisplayOrder,
            input.IsActive,
            input.ChecklistItems.Select(ToDraft).ToArray());
    }

    public static PreventiveChecklistItemDraft ToDraft(PreventiveChecklistItemInput input)
    {
        return new PreventiveChecklistItemDraft(
            input.Id,
            input.ItemKey,
            input.Label,
            ParseItemType(input.ItemType),
            input.DisplayOrder,
            input.IsRequired,
            input.IsActive,
            input.HelpText,
            input.IsCritical,
            input.RequiresCommentOnFailure,
            input.RequiresPhotoOnFailure,
            input.MinimumValue,
            input.MaximumValue,
            input.Options.Select(ToDraft).ToArray());
    }

    public static PreventiveChecklistOptionDraft ToDraft(PreventiveChecklistOptionInput input)
    {
        return new PreventiveChecklistOptionDraft(input.Id, input.Value, input.Label, input.DisplayOrder);
    }

    public static string NormalizeTemplateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        var normalizedCode = code
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');

        while (normalizedCode.Contains("--", StringComparison.Ordinal))
        {
            normalizedCode = normalizedCode.Replace("--", "-", StringComparison.Ordinal);
        }

        return normalizedCode.Trim('-');
    }

    private static PreventiveTemplateSectionDto ToSectionDto(
        PreventiveTemplateSection section,
        bool activeOnly)
    {
        var checklistItems = section.ChecklistItems.AsEnumerable();

        if (activeOnly)
        {
            checklistItems = checklistItems.Where(x => x.IsActive);
        }

        return new PreventiveTemplateSectionDto(
            section.Id,
            section.Title,
            section.DisplayOrder,
            section.IsActive,
            checklistItems
                .OrderBy(x => x.DisplayOrder)
                .Select(ToChecklistItemDto)
                .ToArray());
    }

    private static PreventiveChecklistItemDto ToChecklistItemDto(PreventiveChecklistItem checklistItem)
    {
        return new PreventiveChecklistItemDto(
            checklistItem.Id,
            checklistItem.ItemKey,
            checklistItem.Label,
            PreventiveChecklistItemTypeCatalog.ToValue(checklistItem.ItemType),
            checklistItem.DisplayOrder,
            checklistItem.IsRequired,
            checklistItem.IsActive,
            checklistItem.HelpText,
            checklistItem.IsCritical,
            checklistItem.RequiresCommentOnFailure,
            checklistItem.RequiresPhotoOnFailure,
            checklistItem.MinimumValue,
            checklistItem.MaximumValue,
            checklistItem.Options
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PreventiveChecklistOptionDto(x.Id, x.Value, x.Label, x.DisplayOrder))
                .ToArray());
    }

    private static InfraOps.Domain.PreventiveTemplates.Enums.PreventiveChecklistItemType ParseItemType(string value)
    {
        if (!PreventiveChecklistItemTypeCatalog.TryParse(value, out var itemType))
        {
            throw new InvalidOperationException($"Unsupported preventive checklist item type '{value}'.");
        }

        return itemType;
    }
}
