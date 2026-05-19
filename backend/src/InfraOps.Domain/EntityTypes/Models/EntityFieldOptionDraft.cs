namespace InfraOps.Domain.EntityTypes.Models;

public sealed record EntityFieldOptionDraft(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
