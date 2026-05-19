namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveTemplateFormMetadataDto(
    IReadOnlyCollection<PreventiveTemplateEntityTypeOptionDto> EntityTypes);
