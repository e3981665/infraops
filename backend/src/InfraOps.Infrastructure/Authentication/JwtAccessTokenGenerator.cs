using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InfraOps.Application.Identity;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InfraOps.Infrastructure.Authentication;

public sealed class JwtAccessTokenGenerator : IAccessTokenGenerator
{
    private readonly AuthenticationOptions _authenticationOptions;

    public JwtAccessTokenGenerator(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public GeneratedAccessToken Generate(User user, DateTimeOffset issuedAtUtc)
    {
        var expiresAtUtc = issuedAtUtc.AddMinutes(_authenticationOptions.AccessTokenLifetimeMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = BuildClaims(user);

        var token = new JwtSecurityToken(
            issuer: _authenticationOptions.Issuer,
            audience: _authenticationOptions.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new GeneratedAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }

    private static IReadOnlyCollection<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(user.GetRoleNames().Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.GetPermissionCodes().Select(permission => new Claim(AuthClaimTypes.Permission, permission)));

        return claims;
    }
}
