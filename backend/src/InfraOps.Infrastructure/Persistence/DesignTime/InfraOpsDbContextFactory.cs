using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InfraOps.Infrastructure.Persistence.DesignTime;

public sealed class InfraOpsDbContextFactory : IDesignTimeDbContextFactory<InfraOpsDbContext>
{
    public InfraOpsDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(ResolveConfigurationBasePath())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("InfraOps")
            ?? throw new InvalidOperationException("The 'InfraOps' connection string is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<InfraOpsDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(InfraOpsDbContext).Assembly.FullName));

        return new InfraOpsDbContext(optionsBuilder.Options);
    }

    private static string ResolveConfigurationBasePath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "InfraOps.Api")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InfraOps.Api"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not resolve the API configuration directory for design-time services.");
    }
}
