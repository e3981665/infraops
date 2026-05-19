using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Domain.Tests.Identity.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Should_RevokeRefreshToken_When_TokenIsActive()
    {
        var issuedAt = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
        var refreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hash-value",
            issuedAt,
            issuedAt.AddDays(7));

        refreshToken.Revoke(issuedAt.AddMinutes(5), "replacement-hash");

        Assert.False(refreshToken.IsActive(issuedAt.AddMinutes(6)));
        Assert.Equal("replacement-hash", refreshToken.ReplacedByTokenHash);
    }
}
