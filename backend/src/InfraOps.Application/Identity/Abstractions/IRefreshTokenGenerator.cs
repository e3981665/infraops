using InfraOps.Application.Identity.Dtos;

namespace InfraOps.Application.Identity.Abstractions;

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate(DateTimeOffset issuedAtUtc);
}
