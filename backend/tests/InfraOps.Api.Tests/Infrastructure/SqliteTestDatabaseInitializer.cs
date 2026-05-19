using InfraOps.Infrastructure.Persistence;
using InfraOps.Infrastructure.Persistence.Seeding;

namespace InfraOps.Api.Tests.Infrastructure;

public sealed class SqliteTestDatabaseInitializer : IApplicationDatabaseInitializer
{
    private readonly InfraOpsDbContext _dbContext;
    private readonly LocationDataSeeder _locationDataSeeder;
    private readonly IdentityDataSeeder _identityDataSeeder;

    public SqliteTestDatabaseInitializer(
        InfraOpsDbContext dbContext,
        LocationDataSeeder locationDataSeeder,
        IdentityDataSeeder identityDataSeeder)
    {
        _dbContext = dbContext;
        _locationDataSeeder = locationDataSeeder;
        _identityDataSeeder = identityDataSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await _locationDataSeeder.SeedAsync(cancellationToken);
        await _identityDataSeeder.SeedAsync(cancellationToken);
    }
}
