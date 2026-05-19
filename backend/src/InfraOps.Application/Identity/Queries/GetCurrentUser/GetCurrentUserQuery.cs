using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Identity.Dtos;

namespace InfraOps.Application.Identity.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<CurrentUserDto>;
