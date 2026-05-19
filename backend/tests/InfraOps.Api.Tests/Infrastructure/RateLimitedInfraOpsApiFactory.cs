using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Infrastructure.Persistence;
using InfraOps.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InfraOps.Api.Tests.Infrastructure;

public sealed class RateLimitedInfraOpsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:AuthSensitive:PermitLimit"] = "2",
                ["RateLimiting:AuthSensitive:WindowSeconds"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InfraOpsDbContext>>();
            services.RemoveAll<InfraOpsDbContext>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IApplicationDatabaseInitializer>();

            services.AddDbContext<InfraOpsDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<InfraOpsDbContext>());
            services.AddScoped<IApplicationDatabaseInitializer, SqliteTestDatabaseInitializer>();
        });
    }

    public Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");

        return _connection.OpenAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
        Dispose();
    }
}
