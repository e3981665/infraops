namespace InfraOps.Api.Contracts.Requests.Auth;

public sealed record LogoutRequest(
    string RefreshToken);
