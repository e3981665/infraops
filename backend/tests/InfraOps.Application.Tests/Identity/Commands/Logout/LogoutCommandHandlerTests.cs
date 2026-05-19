using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Commands.Logout;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Tests.Identity.Commands.Logout;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Should_RevokeRefreshToken_When_RefreshTokenIsActive()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        user.AddRefreshToken(RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            "hashed::refresh-token",
            new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero)));

        var handler = new LogoutCommandHandler(
            new LogoutCommandValidator(),
            new StubUserRepository(user),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        await handler.Handle(new LogoutCommand("refresh-token"), CancellationToken.None);

        Assert.NotNull(user.FindRefreshToken("hashed::refresh-token")?.RevokedAtUtc);
    }

    [Fact]
    public async Task Should_IgnoreLogout_When_RefreshTokenDoesNotExist()
    {
        var handler = new LogoutCommandHandler(
            new LogoutCommandValidator(),
            new StubUserRepository(null),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        await handler.Handle(new LogoutCommand("missing-refresh-token"), CancellationToken.None);
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
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(_user);
        }
    }

    private sealed class StubRefreshTokenHasher : IRefreshTokenHasher
    {
        public string Hash(string refreshToken)
        {
            return $"hashed::{refreshToken}";
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 4, 1, 12, 5, 0, TimeSpan.Zero);
    }
}
