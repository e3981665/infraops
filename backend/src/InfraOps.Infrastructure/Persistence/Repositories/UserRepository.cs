using InfraOps.Application.Identity.Abstractions;
using InfraOps.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly InfraOpsDbContext _dbContext;

    public UserRepository(InfraOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return IdentityGraph()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return IdentityGraph()
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        return IdentityGraph()
            .SingleOrDefaultAsync(
                x => x.RefreshTokens.Any(refreshToken => refreshToken.TokenHash == refreshTokenHash),
                cancellationToken);
    }

    private IQueryable<User> IdentityGraph()
    {
        return _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .ThenInclude(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .Include(x => x.RefreshTokens);
    }
}
