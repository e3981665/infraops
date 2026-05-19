using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionById;

public sealed class GetPreventiveExecutionByIdQueryHandler
    : IQueryHandler<GetPreventiveExecutionByIdQuery, PreventiveExecutionDetailsDto>
{
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;

    public GetPreventiveExecutionByIdQueryHandler(IPreventiveExecutionRepository preventiveExecutionRepository)
    {
        _preventiveExecutionRepository = preventiveExecutionRepository;
    }

    public async Task<PreventiveExecutionDetailsDto> Handle(
        GetPreventiveExecutionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var execution = await _preventiveExecutionRepository.GetByIdAsync(query.PreventiveExecutionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive execution was not found.");

        return PreventiveExecutionMappings.ToDetailsDto(execution);
    }
}
