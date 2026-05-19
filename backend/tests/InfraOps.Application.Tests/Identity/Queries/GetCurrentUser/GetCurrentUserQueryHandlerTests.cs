using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Queries.GetCurrentUser;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Tests.Identity.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Should_ReturnCurrentUser_When_IdentityIsAuthenticated()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var permission = new Permission(Guid.NewGuid(), "users.read", "Read users");
        var role = new Role(Guid.NewGuid(), "Admin", "System administrator");
        role.GrantPermission(permission);
        user.AssignRole(role);

        var handler = new GetCurrentUserQueryHandler(
            new StubCurrentUser(user.Id),
            new StubUserRepository(user));

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Contains("users.read", result.Permissions);
    }

    [Fact]
    public async Task Should_RejectCurrentUser_When_UserIsNotAuthenticated()
    {
        var handler = new GetCurrentUserQueryHandler(
            new StubCurrentUser(null),
            new StubUserRepository(null));

        await Assert.ThrowsAsync<ApplicationUnauthorizedException>(() => handler.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None));
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        public StubCurrentUser(Guid? userId)
        {
            UserId = userId;
        }

        public bool IsAuthenticated => UserId is not null;

        public Guid? UserId { get; }
    }

    private sealed class StubUserRepository : IUserRepository
    {
        private readonly User? _user;

        public StubUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_user);
        }

        public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }
    }
}
