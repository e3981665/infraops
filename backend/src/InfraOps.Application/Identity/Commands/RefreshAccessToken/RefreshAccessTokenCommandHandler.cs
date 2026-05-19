using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Identity.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler
    : ICommandHandler<RefreshAccessTokenCommand, AuthenticationTokensDto>
{
    private readonly IValidator<RefreshAccessTokenCommand> _validator;
    private readonly IUserRepository _userRepository;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RefreshAccessTokenCommandHandler(
        IValidator<RefreshAccessTokenCommand> validator,
        IUserRepository userRepository,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _validator = validator;
        _userRepository = userRepository;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthenticationTokensDto> Handle(
        RefreshAccessTokenCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var refreshTokenHash = _refreshTokenHasher.Hash(command.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new ApplicationUnauthorizedException("Refresh token is invalid.");
        }

        var existingRefreshToken = user.FindRefreshToken(refreshTokenHash);
        var now = _clock.UtcNow;

        if (existingRefreshToken is null || !existingRefreshToken.IsActive(now))
        {
            throw new ApplicationUnauthorizedException("Refresh token is invalid.");
        }

        var accessToken = _accessTokenGenerator.Generate(user, now);
        var newRefreshToken = _refreshTokenGenerator.Generate(now);
        var newRefreshTokenHash = _refreshTokenHasher.Hash(newRefreshToken.Token);

        existingRefreshToken.Revoke(now, newRefreshTokenHash);
        user.AddRefreshToken(RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            now,
            newRefreshToken.ExpiresAtUtc));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationTokensDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAtUtc);
    }
}
