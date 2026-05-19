namespace InfraOps.Application.Identity.Dtos;

public sealed record GeneratedAccessToken(
    string Token,
    DateTimeOffset ExpiresAtUtc);
