using System.Security.Cryptography;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Dtos;
using Microsoft.Extensions.Options;

namespace InfraOps.Infrastructure.Authentication;

public sealed class SecureRefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly AuthenticationOptions _authenticationOptions;

    public SecureRefreshTokenGenerator(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public GeneratedRefreshToken Generate(DateTimeOffset issuedAtUtc)
    {
        var rawToken = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(rawToken)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new GeneratedRefreshToken(
            refreshToken,
            issuedAtUtc.AddDays(_authenticationOptions.RefreshTokenLifetimeDays));
    }
}
