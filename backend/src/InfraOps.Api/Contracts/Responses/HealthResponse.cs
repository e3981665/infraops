namespace InfraOps.Api.Contracts.Responses;

public sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc);
