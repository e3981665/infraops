namespace InfraOps.Application.EntityTypes.Support;

public sealed record EntityFieldOptionInput(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
