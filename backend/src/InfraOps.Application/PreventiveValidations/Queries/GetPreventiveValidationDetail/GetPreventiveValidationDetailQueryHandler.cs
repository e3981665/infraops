using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveValidations.Queries.GetPreventiveValidationDetail;

public sealed class GetPreventiveValidationDetailQueryHandler
    : IQueryHandler<GetPreventiveValidationDetailQuery, PreventiveExecutionDetailsDto>
{
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;

    public GetPreventiveValidationDetailQueryHandler(
        IPreventiveExecutionRepository preventiveExecutionRepository)
    {
        _preventiveExecutionRepository = preventiveExecutionRepository;
    }

    public async Task<PreventiveExecutionDetailsDto> Handle(
        GetPreventiveValidationDetailQuery query,
        CancellationToken cancellationToken)
    {
        var execution = await _preventiveExecutionRepository.GetByIdAsync(query.PreventiveExecutionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive execution was not found.");

        return PreventiveExecutionMappings.ToDetailsDto(execution);
    }
}
