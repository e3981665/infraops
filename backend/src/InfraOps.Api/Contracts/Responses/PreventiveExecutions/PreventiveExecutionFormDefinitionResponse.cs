namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionFormDefinitionResponse(
    Guid InventoryItemId,
    string InventoryItemDisplayName,
    Guid EntityTypeId,
    string EntityTypeName,
    string EntityTypeCode,
    Guid PreventiveTemplateId,
    string PreventiveTemplateName,
    string PreventiveTemplateCode,
    IReadOnlyCollection<PreventiveExecutionTemplateSectionResponse> Sections);
