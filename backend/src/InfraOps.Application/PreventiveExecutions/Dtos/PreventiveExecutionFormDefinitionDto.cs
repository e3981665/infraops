namespace InfraOps.Application.PreventiveExecutions.Dtos;

public sealed record PreventiveExecutionFormDefinitionDto(
    Guid InventoryItemId,
    string InventoryItemDisplayName,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    Guid PreventiveTemplateId,
    string PreventiveTemplateName,
    string PreventiveTemplateCode,
    IReadOnlyCollection<PreventiveExecutionTemplateSectionDto> Sections);
