namespace InfraOps.Application.PreventiveTemplates.Dtos;

public sealed record PreventiveChecklistOptionDto(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
