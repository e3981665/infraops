using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Domain.PreventiveExecutions.Entities;

namespace InfraOps.Application.PreventiveExecutions.Abstractions;

public interface IPreventiveExecutionRepository
{
    Task AddAsync(PreventiveExecution preventiveExecution, CancellationToken cancellationToken);

    Task<PreventiveExecution?> GetByIdAsync(Guid preventiveExecutionId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PreventiveExecution>> ListAsync(
        PreventiveExecutionListFilter filter,
        CancellationToken cancellationToken);
}
