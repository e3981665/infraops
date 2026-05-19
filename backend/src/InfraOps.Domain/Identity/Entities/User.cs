using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.Identity.Entities;

public sealed class User
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    private User(Guid id, string fullName, string email, string normalizedEmail, string passwordHash)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public static User Create(Guid id, string fullName, string email, string passwordHash)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User id is required.");
        }

        var normalizedFullName = NormalizeFullName(fullName);
        var normalizedEmail = NormalizeEmail(email);
        var normalizedPasswordHash = NormalizePasswordHash(passwordHash);

        return new User(
            id,
            normalizedFullName,
            email.Trim(),
            normalizedEmail,
            normalizedPasswordHash);
    }

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainRuleException("User email is required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!normalizedEmail.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainRuleException("User email must be valid.");
        }

        return normalizedEmail;
    }

    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (_userRoles.Any(x => x.RoleId == role.Id))
        {
            return;
        }

        _userRoles.Add(new UserRole(Id, role.Id, this, role));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = NormalizePasswordHash(passwordHash);
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        _refreshTokens.Add(refreshToken);
    }

    public RefreshToken? FindRefreshToken(string tokenHash)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        return _refreshTokens.SingleOrDefault(x => x.TokenHash == tokenHash);
    }

    public IReadOnlyCollection<string> GetRoleNames()
    {
        return _userRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyCollection<string> GetPermissionCodes()
    {
        return _userRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainRuleException("User full name is required.");
        }

        return fullName.Trim();
    }

    private static string NormalizePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainRuleException("User password hash is required.");
        }

        return passwordHash.Trim();
    }
}
