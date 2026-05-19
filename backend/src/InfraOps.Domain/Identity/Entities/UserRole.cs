namespace InfraOps.Domain.Identity.Entities;

public sealed class UserRole
{
    private UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId, User user, Role role)
    {
        UserId = userId;
        RoleId = roleId;
        User = user;
        Role = role;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public User User { get; private set; } = null!;

    public Role Role { get; private set; } = null!;
}
