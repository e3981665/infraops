namespace InfraOps.Api.Contracts.Responses.PreventiveTemplates;

public sealed record PreventiveTemplateFormMetadataResponse(
    IReadOnlyCollection<PreventiveTemplateEntityTypeOptionResponse> EntityTypes);
