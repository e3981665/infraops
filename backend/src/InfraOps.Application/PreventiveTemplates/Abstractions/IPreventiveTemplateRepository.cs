using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Application.PreventiveTemplates.Abstractions;

public interface IPreventiveTemplateRepository
{
    Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken);

    Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PreventiveTemplate>> ListAsync(
        PreventiveTemplateListFilter filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PreventiveTemplate>> ListByEntityTypeAsync(
        Guid entityTypeId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<bool> IsCodeInUseAsync(
        string normalizedCode,
        Guid? excludedPreventiveTemplateId,
        CancellationToken cancellationToken);
}
