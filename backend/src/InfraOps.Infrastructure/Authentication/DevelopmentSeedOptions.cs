namespace InfraOps.Infrastructure.Authentication;

public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "DevelopmentSeed";

    // Local/demo seed account only. Replace through configuration outside development.
    public string AdminFullName { get; init; } = "InfraOps Administrator";

    public string AdminEmail { get; init; } = "admin@infraops.local";

    public string AdminPassword { get; init; } = "DemoOnly-Admin-Local";

    public string DemoContentLocale { get; init; } = "en-US";
}
