using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Locations.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Infrastructure.Authentication;
using InfraOps.Infrastructure.Persistence;
using InfraOps.Infrastructure.Persistence.Repositories;
using InfraOps.Infrastructure.Persistence.Seeding;
using InfraOps.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfraOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InfraOps")
            ?? throw new InvalidOperationException("The 'InfraOps' connection string is not configured.");

        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.Configure<DevelopmentSeedOptions>(configuration.GetSection(DevelopmentSeedOptions.SectionName));

        services.AddDbContext<InfraOpsDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(InfraOpsDbContext).Assembly.FullName);
                });
        });

        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IRefreshTokenHasher, Sha256RefreshTokenHasher>();
        services.AddScoped<IRefreshTokenGenerator, SecureRefreshTokenGenerator>();
        services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardMetricsRepository, DashboardMetricsRepository>();
        services.AddScoped<IEntityTypeRepository, EntityTypeRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IPreventiveExecutionRepository, PreventiveExecutionRepository>();
        services.AddScoped<IPreventiveTemplateRepository, PreventiveTemplateRepository>();
        services.AddScoped<ILocationLookupRepository, LocationLookupRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<InfraOpsDbContext>());
        services.AddScoped<LocationDataSeeder>();
        services.AddScoped<IdentityDataSeeder>();
        services.AddScoped<DemoDataSeeder>();
        services.AddScoped<IApplicationDatabaseInitializer, ApplicationDatabaseInitializer>();

        return services;
    }
}
