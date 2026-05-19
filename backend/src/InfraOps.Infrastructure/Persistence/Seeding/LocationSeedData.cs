namespace InfraOps.Infrastructure.Persistence.Seeding;

public static class LocationSeedData
{
    public static IReadOnlyCollection<(Guid Id, string Code, string Name)> Regions { get; } =
    [
        (Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"), "north-region", "North Region"),
        (Guid.Parse("A17F351D-775A-46CD-8C6D-03304E84C6E1"), "south-region", "South Region")
    ];

    public static IReadOnlyCollection<(Guid Id, Guid RegionId, string Code, string Name)> Sites { get; } =
    [
        (
            Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"),
            Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"),
            "north-hub",
            "North Hub"),
        (
            Guid.Parse("4D6F1708-1C49-45B2-B31D-4994B86E286C"),
            Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"),
            "riverside-station",
            "Riverside Station"),
        (
            Guid.Parse("CC63E481-4242-4CA1-BC3F-6D1B6924F69F"),
            Guid.Parse("A17F351D-775A-46CD-8C6D-03304E84C6E1"),
            "south-yard",
            "South Yard")
    ];
}
