using InfraOps.Api;
using InfraOps.Api.Extensions;
using InfraOps.Application;
using InfraOps.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeInfrastructureAsync();

app.UsePresentation();

await app.RunAsync();

public partial class Program;
