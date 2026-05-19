using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Commands.Login;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Tests.Identity.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Should_Login_When_CredentialsAreValid()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var handler = new LoginCommandHandler(
            new LoginCommandValidator(),
            new StubUserRepository(user, null, null),
            new StubPasswordHasher(true),
            new StubAccessTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        var result = await handler.Handle(
            new LoginCommand("admin@infraops.local", "DemoOnly-Admin-Local"),
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task Should_RejectLogin_When_PasswordIsInvalid()
    {
        var user = User.Create(Guid.NewGuid(), "Admin User", "admin@infraops.local", "hashed-password");
        var handler = new LoginCommandHandler(
            new LoginCommandValidator(),
            new StubUserRepository(user, null, null),
            new StubPasswordHasher(false),
            new StubAccessTokenGenerator(),
            new StubRefreshTokenGenerator(),
            new StubRefreshTokenHasher(),
            new StubUnitOfWork(),
            new StubClock());

        await Assert.ThrowsAsync<ApplicationUnauthorizedException>(() => handler.Handle(
            new LoginCommand("admin@infraops.local", "wrong-password"),
            CancellationToken.None));
    }

    private sealed class StubUserRepository : IUserRepository
    {
        private readonly User? _userByEmail;
        private readonly User? _userById;
        private readonly User? _userByRefreshToken;

        public StubUserRepository(User? userByEmail, User? userById, User? userByRefreshToken)
        {
            _userByEmail = userByEmail;
            _userById = userById;
            _userByRefreshToken = userByRefreshToken;
        }

        public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userByEmail);
        }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userById);
        }

        public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(_userByRefreshToken);
        }
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        private readonly bool _verificationResult;

        public StubPasswordHasher(bool verificationResult)
        {
            _verificationResult = verificationResult;
        }

        public string Hash(string password)
        {
            return "hashed-password";
        }

        public bool Verify(string password, string passwordHash)
        {
            return _verificationResult;
        }
    }

    private sealed class StubAccessTokenGenerator : IAccessTokenGenerator
    {
        public GeneratedAccessToken Generate(User user, DateTimeOffset issuedAtUtc)
        {
            return new GeneratedAccessToken("access-token", issuedAtUtc.AddMinutes(15));
        }
    }

    private sealed class StubRefreshTokenGenerator : IRefreshTokenGenerator
    {
        public GeneratedRefreshToken Generate(DateTimeOffset issuedAtUtc)
        {
            return new GeneratedRefreshToken("refresh-token", issuedAtUtc.AddDays(7));
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
        public DateTimeOffset UtcNow => new(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
    }
}
