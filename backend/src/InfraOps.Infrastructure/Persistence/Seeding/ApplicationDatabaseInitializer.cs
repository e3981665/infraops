using Microsoft.EntityFrameworkCore;

namespace InfraOps.Infrastructure.Persistence.Seeding;

public sealed class ApplicationDatabaseInitializer : IApplicationDatabaseInitializer
{
    private readonly InfraOpsDbContext _dbContext;
    private readonly LocationDataSeeder _locationDataSeeder;
    private readonly IdentityDataSeeder _identityDataSeeder;
    private readonly DemoDataSeeder _demoDataSeeder;

    public ApplicationDatabaseInitializer(
        InfraOpsDbContext dbContext,
        LocationDataSeeder locationDataSeeder,
        IdentityDataSeeder identityDataSeeder,
        DemoDataSeeder demoDataSeeder)
    {
        _dbContext = dbContext;
        _locationDataSeeder = locationDataSeeder;
        _identityDataSeeder = identityDataSeeder;
        _demoDataSeeder = demoDataSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await _locationDataSeeder.SeedAsync(cancellationToken);
        await _identityDataSeeder.SeedAsync(cancellationToken);
        await _demoDataSeeder.SeedAsync(cancellationToken);
    }
}
