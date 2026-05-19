namespace InfraOps.Application.Identity.Abstractions;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);
}
