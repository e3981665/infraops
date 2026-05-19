using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Identity.Abstractions;

namespace InfraOps.Application.Identity.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IValidator<LogoutCommand> _validator;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public LogoutCommandHandler(
        IValidator<LogoutCommand> validator,
        IUserRepository userRepository,
        IRefreshTokenHasher refreshTokenHasher,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _validator = validator;
        _userRepository = userRepository;
        _refreshTokenHasher = refreshTokenHasher;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var refreshTokenHash = _refreshTokenHasher.Hash(command.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (user is null)
        {
            return;
        }

        var refreshToken = user.FindRefreshToken(refreshTokenHash);

        if (refreshToken is null || !refreshToken.IsActive(_clock.UtcNow))
        {
            return;
        }

        refreshToken.Revoke(_clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
