using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Identity.Dtos;

namespace InfraOps.Application.Identity.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<AuthenticationTokensDto>;
