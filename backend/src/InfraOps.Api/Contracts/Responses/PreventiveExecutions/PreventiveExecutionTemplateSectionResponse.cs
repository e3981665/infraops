namespace InfraOps.Api.Contracts.Responses.PreventiveExecutions;

public sealed record PreventiveExecutionTemplateSectionResponse(
    Guid Id,
    Guid SourceTemplateSectionId,
    string Title,
    int DisplayOrder,
    IReadOnlyCollection<PreventiveExecutionTemplateItemResponse> ChecklistItems);
