using InfraOps.Application.Abstractions.Messaging;

namespace InfraOps.Application.Diagnostics.Queries.GetApplicationInfo;

public sealed record GetApplicationInfoQuery : IQuery<ApplicationInfoResponse>;
