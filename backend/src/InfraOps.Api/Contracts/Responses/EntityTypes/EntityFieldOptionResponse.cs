namespace InfraOps.Api.Contracts.Responses.EntityTypes;

public sealed record EntityFieldOptionResponse(
    Guid Id,
    string Value,
    string Label,
    int DisplayOrder);
