using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Identity.Dtos;

namespace InfraOps.Application.Identity.Commands.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand(
    string RefreshToken) : ICommand<AuthenticationTokensDto>;
