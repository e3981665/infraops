using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Identity.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthenticationTokensDto>
{
    private readonly IValidator<LoginCommand> _validator;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public LoginCommandHandler(
        IValidator<LoginCommand> validator,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _validator = validator;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthenticationTokensDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedEmail = User.NormalizeEmail(command.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new ApplicationUnauthorizedException("Invalid credentials.");
        }

        var now = _clock.UtcNow;
        var accessToken = _accessTokenGenerator.Generate(user, now);
        var refreshToken = _refreshTokenGenerator.Generate(now);
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken.Token);

        user.AddRefreshToken(RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            now,
            refreshToken.ExpiresAtUtc));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationTokensDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }
}
