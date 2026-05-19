using InfraOps.Application.Identity.Abstractions;
using InfraOps.Domain.Identity.Constants;
using InfraOps.Domain.Identity.Entities;
using InfraOps.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InfraOps.Infrastructure.Persistence.Seeding;

public sealed class IdentityDataSeeder
{
    private readonly InfraOpsDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly DevelopmentSeedOptions _developmentSeedOptions;

    public IdentityDataSeeder(
        InfraOpsDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<DevelopmentSeedOptions> developmentSeedOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _developmentSeedOptions = developmentSeedOptions.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await EnsurePermissionsAsync(cancellationToken);
        await EnsureRolesAsync(cancellationToken);
        await EnsureAdminUserAsync(cancellationToken);
        await EnsureDemoUsersAsync(cancellationToken);
    }

    private async Task EnsurePermissionsAsync(CancellationToken cancellationToken)
    {
        var existingPermissions = await _dbContext.Permissions
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var (code, description) in IdentitySeedData.Permissions)
        {
            if (existingPermissions.ContainsKey(code))
            {
                continue;
            }

            _dbContext.Permissions.Add(new Permission(Guid.NewGuid(), code, description));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync(CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var roles = await _dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var (name, description, permissionCodes) in IdentitySeedData.Roles)
        {
            if (!roles.TryGetValue(name, out var role))
            {
                role = new Role(Guid.NewGuid(), name, description);
                _dbContext.Roles.Add(role);
                roles[name] = role;
            }

            foreach (var permissionCode in permissionCodes)
            {
                role.GrantPermission(permissions[permissionCode]);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAdminUserAsync(CancellationToken cancellationToken)
    {
        var normalizedAdminEmail = User.NormalizeEmail(_developmentSeedOptions.AdminEmail);
        var adminRole = await _dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .SingleAsync(x => x.Name == RoleNames.Admin, cancellationToken);

        var adminUser = await _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedAdminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = User.Create(
                Guid.NewGuid(),
                _developmentSeedOptions.AdminFullName,
                _developmentSeedOptions.AdminEmail,
                _passwordHasher.Hash(_developmentSeedOptions.AdminPassword));

            adminUser.AssignRole(adminRole);
            _dbContext.Users.Add(adminUser);
        }
        else
        {
            adminUser.Activate();
            adminUser.AssignRole(adminRole);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDemoUsersAsync(CancellationToken cancellationToken)
    {
        // Local/demo seed users only. These obvious fake passwords must not be reused in real deployments.
        await EnsureUserWithRoleAsync(
            "InfraOps Technician",
            "technician@infraops.local",
            "DemoOnly-Tech-Local",
            RoleNames.Technician,
            cancellationToken);

        await EnsureUserWithRoleAsync(
            "InfraOps Validator",
            "validator@infraops.local",
            "DemoOnly-Validator-Local",
            RoleNames.Validator,
            cancellationToken);
    }

    private async Task EnsureUserWithRoleAsync(
        string fullName,
        string email,
        string password,
        string roleName,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(email);
        var role = await _dbContext.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .SingleAsync(x => x.Name == roleName, cancellationToken);

        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = User.Create(
                Guid.NewGuid(),
                fullName,
                email,
                _passwordHasher.Hash(password));

            user.AssignRole(role);
            _dbContext.Users.Add(user);
        }
        else
        {
            user.Activate();
            user.AssignRole(role);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
