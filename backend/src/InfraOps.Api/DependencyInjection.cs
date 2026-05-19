using System.Threading.RateLimiting;
using System.Security.Claims;
using System.Text;
using InfraOps.Api.Authorization;
using InfraOps.Application.Identity;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Domain.Identity.Constants;
using InfraOps.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace InfraOps.Api;

public static class DependencyInjection
{
    private const string FrontendCorsPolicy = "Frontend";
    public const string AuthSensitiveRateLimitPolicy = "AuthSensitive";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        allowedOrigins ??=
        [
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:4173",
            "http://127.0.0.1:4173"
        ];

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthSensitiveRateLimitPolicy, context =>
            {
                var permitLimit = configuration.GetValue<int?>("RateLimiting:AuthSensitive:PermitLimit") ?? 30;
                var windowSeconds = configuration.GetValue<int?>("RateLimiting:AuthSensitive:WindowSeconds") ?? 60;
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(windowSeconds)
                    });
            });
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionCodes.All)
            {
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new PermissionRequirement(permission));
                });
            }
        });

        var authenticationOptions = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authenticationOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authenticationOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        return services;
    }

    public static string GetFrontendCorsPolicyName()
    {
        return FrontendCorsPolicy;
    }
}
