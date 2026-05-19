using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Models;

namespace InfraOps.Application.EntityTypes.Support;

public static class EntityTypeMappings
{
    public static EntityTypeDetailsDto ToDetailsDto(EntityType entityType)
    {
        return new EntityTypeDetailsDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.Description,
            entityType.IsActive,
            entityType.FieldDefinitions
                .OrderBy(x => x.DisplayOrder)
                .Select(ToFieldDefinitionDto)
                .ToArray());
    }

    public static EntityTypeSummaryDto ToSummaryDto(EntityType entityType)
    {
        return new EntityTypeSummaryDto(
            entityType.Id,
            entityType.Name,
            entityType.Code,
            entityType.Description,
            entityType.IsActive,
            entityType.FieldDefinitions.Count(x => x.IsActive));
    }

    public static EntityFieldDefinitionDraft ToDraft(EntityFieldDefinitionInput input)
    {
        return new EntityFieldDefinitionDraft(
            input.Id,
            input.FieldKey,
            input.DisplayLabel,
            ParseFieldType(input.FieldType),
            input.DisplayOrder,
            input.IsRequired,
            input.IsActive,
            input.Placeholder,
            input.HelpText,
            input.Options.Select(ToDraft).ToArray());
    }

    public static EntityFieldOptionDraft ToDraft(EntityFieldOptionInput input)
    {
        return new EntityFieldOptionDraft(input.Id, input.Value, input.Label, input.DisplayOrder);
    }

    private static EntityFieldDefinitionDto ToFieldDefinitionDto(EntityFieldDefinition fieldDefinition)
    {
        return new EntityFieldDefinitionDto(
            fieldDefinition.Id,
            fieldDefinition.FieldKey,
            fieldDefinition.DisplayLabel,
            EntityFieldTypeCatalog.ToValue(fieldDefinition.FieldType),
            fieldDefinition.DisplayOrder,
            fieldDefinition.IsRequired,
            fieldDefinition.IsActive,
            fieldDefinition.Placeholder,
            fieldDefinition.HelpText,
            fieldDefinition.Options
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new EntityFieldOptionDto(x.Id, x.Value, x.Label, x.DisplayOrder))
                .ToArray());
    }

    private static InfraOps.Domain.EntityTypes.Enums.EntityFieldType ParseFieldType(string value)
    {
        if (!EntityFieldTypeCatalog.TryParse(value, out var fieldType))
        {
            throw new InvalidOperationException($"Unsupported field type '{value}'.");
        }

        return fieldType;
    }
}
