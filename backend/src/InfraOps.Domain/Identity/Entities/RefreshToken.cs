using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.Identity.Entities;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Issue(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Refresh token id is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainRuleException("Refresh token user id is required.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainRuleException("Refresh token hash is required.");
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new DomainRuleException("Refresh token expiration must be after creation.");
        }

        return new RefreshToken(id, userId, tokenHash.Trim(), createdAtUtc, expiresAtUtc);
    }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > now;
    }

    public void Revoke(DateTimeOffset revokedAtUtc, string? replacedByTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            throw new DomainRuleException("Refresh token has already been revoked.");
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = string.IsNullOrWhiteSpace(replacedByTokenHash)
            ? null
            : replacedByTokenHash.Trim();
    }
}
