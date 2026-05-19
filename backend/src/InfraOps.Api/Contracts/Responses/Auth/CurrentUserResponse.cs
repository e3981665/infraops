namespace InfraOps.Api.Contracts.Responses.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
