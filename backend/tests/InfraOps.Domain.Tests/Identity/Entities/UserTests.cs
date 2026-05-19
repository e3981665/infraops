using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Domain.Tests.Identity.Entities;

public sealed class UserTests
{
    [Fact]
    public void Should_AssignRole_When_RoleIsNotAlreadyAssigned()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var role = new Role(Guid.NewGuid(), "Admin", "System administrator");

        user.AssignRole(role);

        Assert.Single(user.UserRoles);
        Assert.Equal("Admin", user.GetRoleNames().Single());
    }

    [Fact]
    public void Should_NotDuplicateRole_When_RoleIsAssignedTwice()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var role = new Role(Guid.NewGuid(), "Admin", "System administrator");

        user.AssignRole(role);
        user.AssignRole(role);

        Assert.Single(user.UserRoles);
    }
}
