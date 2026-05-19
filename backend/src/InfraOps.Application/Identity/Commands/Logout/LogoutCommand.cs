using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.Identity.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken) : ICommand;
