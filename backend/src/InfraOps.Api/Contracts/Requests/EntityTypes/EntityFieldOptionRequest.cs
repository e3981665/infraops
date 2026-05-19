namespace InfraOps.Api.Contracts.Requests.EntityTypes;

public sealed record EntityFieldOptionRequest(
    Guid? Id,
    string Value,
    string Label,
    int DisplayOrder);
