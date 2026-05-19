namespace InfraOps.Application.Identity.Dtos;

public sealed record GeneratedRefreshToken(
    string Token,
    DateTimeOffset ExpiresAtUtc);
