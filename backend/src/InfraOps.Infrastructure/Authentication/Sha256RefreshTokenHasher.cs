using System.Security.Cryptography;
using System.Text;
using InfraOps.Application.Identity.Abstractions;

namespace InfraOps.Infrastructure.Authentication;

public sealed class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
