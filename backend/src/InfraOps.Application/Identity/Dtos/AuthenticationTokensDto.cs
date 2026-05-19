namespace InfraOps.Application.Identity.Dtos;

public sealed record AuthenticationTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
