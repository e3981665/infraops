using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.Diagnostics.Queries.GetApplicationInfo;

public sealed class GetApplicationInfoQueryHandler
    : IQueryHandler<GetApplicationInfoQuery, ApplicationInfoResponse>
{
    private static readonly string[] Modules =
    [
        "Identity",
        "Users",
        "Roles",
        "EntityTypes",
        "Inventory",
        "PreventiveTemplates",
        "PreventiveExecutions",
        "PreventiveValidations",
        "Regions",
        "Sites"
    ];

    public Task<ApplicationInfoResponse> Handle(
        GetApplicationInfoQuery query,
        CancellationToken cancellationToken)
    {
        var response = new ApplicationInfoResponse(
            "InfraOps",
            "Clean Architecture",
            Modules);

        return Task.FromResult(response);
    }
}
