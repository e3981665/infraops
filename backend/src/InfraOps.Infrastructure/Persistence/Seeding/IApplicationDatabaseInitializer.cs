namespace InfraOps.Infrastructure.Persistence.Seeding;

public interface IApplicationDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
