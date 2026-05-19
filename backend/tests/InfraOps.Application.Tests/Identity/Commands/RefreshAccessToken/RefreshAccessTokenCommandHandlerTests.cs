using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Commands.RefreshAccessToken;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Tests.Identity.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandlerTests
{
    [Fact]
    public async Task Should_RefreshTokens_When_RefreshTokenIsValid()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var role = new Role(Guid.NewGuid(), "Admin", "System administrator");
        user.AssignRole(role);
        user.AddRefreshToken(RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            "hashed::current-refresh-token",
            new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero)));

        var handler = new RefreshAccessTokenCommandHandler(
            new RefreshAccessTokenCommandValidator(),
            new StubUserRepository(user),
            new StubAccessTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        var result = await handler.Handle(
            new RefreshAccessTokenCommand("current-refresh-token"),
            CancellationToken.None);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal(2, user.RefreshTokens.Count);
    }

    [Fact]
    public async Task Should_RejectRefresh_When_RefreshTokenIsInvalid()
    {
        var handler = new RefreshAccessTokenCommandHandler(
            new RefreshAccessTokenCommandValidator(),
            new StubUserRepository(null),
            new StubAccessTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        await Assert.ThrowsAsync<ApplicationUnauthorizedException>(() => handler.Handle(
            new RefreshAccessTokenCommand("missing-refresh-token"),
            CancellationToken.None));
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

    private sealed class StubAccessTokenGenerator : IAccessTokenGenerator
    {
        public GeneratedAccessToken Generate(User user, DateTimeOffset issuedAtUtc)
        {
            return new GeneratedAccessToken("new-access-token", issuedAtUtc.AddMinutes(15));
        }
    }

    private sealed class StubRefreshTokenGenerator : IRefreshTokenGenerator
    {
        public GeneratedRefreshToken Generate(DateTimeOffset issuedAtUtc)
        {
            return new GeneratedRefreshToken("new-refresh-token", issuedAtUtc.AddDays(7));
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
