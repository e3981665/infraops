namespace InfraOps.Infrastructure.Authentication;

public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "DevelopmentSeed";

    public string AdminFullName { get; init; } = "InfraOps Administrator";

    public string AdminEmail { get; init; } = "admin@infraops.local";

    public string AdminPassword { get; init; } = "InfraOps.Admin!123";
}
