namespace InfraOps.Application.Identity.Dtos;

public sealed record CurrentUserDto(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
