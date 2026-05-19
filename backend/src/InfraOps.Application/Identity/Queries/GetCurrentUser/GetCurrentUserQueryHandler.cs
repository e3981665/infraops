using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Identity.Dtos;

namespace InfraOps.Application.Identity.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new ApplicationUnauthorizedException("The current user is not authenticated.");
        }

        var user = await _userRepository.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new ApplicationUnauthorizedException("The current user is not available.");
        }

        return new CurrentUserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.GetRoleNames(),
            user.GetPermissionCodes());
    }
}
