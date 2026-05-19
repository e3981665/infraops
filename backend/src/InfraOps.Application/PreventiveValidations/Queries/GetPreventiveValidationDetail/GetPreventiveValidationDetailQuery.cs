using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveValidations.Queries.GetPreventiveValidationDetail;

public sealed record GetPreventiveValidationDetailQuery(Guid PreventiveExecutionId)
    : IQuery<PreventiveExecutionDetailsDto>;
