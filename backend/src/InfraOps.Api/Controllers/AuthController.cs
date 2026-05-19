using InfraOps.Api.Contracts.Requests.Auth;
using InfraOps.Api.Contracts.Responses.Auth;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Identity.Commands.Login;
using InfraOps.Application.Identity.Commands.Logout;
using InfraOps.Application.Identity.Commands.RefreshAccessToken;
using InfraOps.Application.Identity.Dtos;
using InfraOps.Application.Identity.Queries.GetCurrentUser;
using InfraOps.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, AuthenticationTokensDto> _loginHandler;
    private readonly ICommandHandler<RefreshAccessTokenCommand, AuthenticationTokensDto> _refreshHandler;
    private readonly ICommandHandler<LogoutCommand> _logoutHandler;
    private readonly IQueryHandler<GetCurrentUserQuery, CurrentUserDto> _currentUserHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ICommandHandler<LoginCommand, AuthenticationTokensDto> loginHandler,
        ICommandHandler<RefreshAccessTokenCommand, AuthenticationTokensDto> refreshHandler,
        ICommandHandler<LogoutCommand> logoutHandler,
        IQueryHandler<GetCurrentUserQuery, CurrentUserDto> currentUserHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
        _logoutHandler = logoutHandler;
        _currentUserHandler = currentUserHandler;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(DependencyInjection.AuthSensitiveRateLimitPolicy)]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthenticationTokensDto result;

        try
        {
            result = await _loginHandler.Handle(
                new LoginCommand(request.Email, request.Password),
                cancellationToken);
        }
        catch (ApplicationUnauthorizedException)
        {
            _logger.LogWarning("Login failed for {Email}.", request.Email);
            throw;
        }

        _logger.LogInformation("Login succeeded for {Email}.", request.Email);

        return Ok(MapTokenResponse(result));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting(DependencyInjection.AuthSensitiveRateLimitPolicy)]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _refreshHandler.Handle(
            new RefreshAccessTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(MapTokenResponse(result));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _logoutHandler.Handle(
            new LogoutCommand(request.RefreshToken),
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await _currentUserHandler.Handle(new GetCurrentUserQuery(), cancellationToken);

        return Ok(new CurrentUserResponse(
            result.Id,
            result.FullName,
            result.Email,
            result.Roles,
            result.Permissions));
    }

    private static TokenResponse MapTokenResponse(AuthenticationTokensDto result)
    {
        return new TokenResponse(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
    }
}
