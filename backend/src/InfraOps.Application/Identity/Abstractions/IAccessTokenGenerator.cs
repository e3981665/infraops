using InfraOps.Application.Identity.Dtos;
using InfraOps.Domain.Identity.Entities;

namespace InfraOps.Application.Identity.Abstractions;

public interface IAccessTokenGenerator
{
    GeneratedAccessToken Generate(User user, DateTimeOffset issuedAtUtc);
}
