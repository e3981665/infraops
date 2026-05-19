using InfraOps.Api;
using InfraOps.Api.Extensions;
using InfraOps.Application;
using InfraOps.Infrastructure;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

try
{
    var app = builder.Build();

    await app.Services.InitializeInfrastructureAsync();

    app.UsePresentation();

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "InfraOps API terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
