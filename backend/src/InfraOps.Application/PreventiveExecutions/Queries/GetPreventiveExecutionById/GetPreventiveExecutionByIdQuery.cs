using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionById;

public sealed record GetPreventiveExecutionByIdQuery(Guid PreventiveExecutionId)
    : IQuery<PreventiveExecutionDetailsDto>;
