namespace InfraOps.Application.Diagnostics.Queries.GetApplicationInfo;

public sealed record ApplicationInfoResponse(
    string ProductName,
    string ArchitectureStyle,
    IReadOnlyCollection<string> Modules);
