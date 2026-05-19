using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.Identity.Entities;

public sealed class Role
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Role()
    {
    }

    public Role(Guid id, string name, string description)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Role id is required.");
        }

        Id = id;
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public void GrantPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (_rolePermissions.Any(x => x.PermissionId == permission.Id))
        {
            return;
        }

        _rolePermissions.Add(new RolePermission(Id, permission.Id, this, permission));
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Role name is required.");
        }

        return name.Trim();
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainRuleException("Role description is required.");
        }

        return description.Trim();
    }
}
