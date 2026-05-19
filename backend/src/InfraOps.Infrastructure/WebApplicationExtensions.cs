using InfraOps.Infrastructure.Persistence.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace InfraOps.Infrastructure;

public static class ServiceProviderExtensions
{
    public static async Task InitializeInfrastructureAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IApplicationDatabaseInitializer>();

        await initializer.InitializeAsync(cancellationToken);
    }
}
