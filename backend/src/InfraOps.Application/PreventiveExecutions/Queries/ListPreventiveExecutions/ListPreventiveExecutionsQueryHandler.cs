using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Queries.ListPreventiveExecutions;

public sealed class ListPreventiveExecutionsQueryHandler
    : IQueryHandler<ListPreventiveExecutionsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>>
{
    private readonly IValidator<ListPreventiveExecutionsQuery> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;
    private readonly ICurrentUser _currentUser;

    public ListPreventiveExecutionsQueryHandler(
        IValidator<ListPreventiveExecutionsQuery> validator,
        IPreventiveExecutionRepository preventiveExecutionRepository,
        ICurrentUser currentUser)
    {
        _validator = validator;
        _preventiveExecutionRepository = preventiveExecutionRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<PreventiveExecutionSummaryDto>> Handle(
        ListPreventiveExecutionsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        PreventiveExecutionStatusCatalog.TryParse(query.Status, out var parsedStatus);

        Guid? createdBy = null;

        if (query.CreatedByCurrentUser)
        {
            createdBy = _currentUser.UserId
                ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");
        }

        var executions = await _preventiveExecutionRepository.ListAsync(
            new PreventiveExecutionListFilter(
                string.IsNullOrWhiteSpace(query.Status) ? null : parsedStatus,
                query.EntityTypeId,
                query.InventoryItemId,
                query.SiteId,
                query.RegionId,
                createdBy,
                query.SubmittedBy,
                query.StartedFromUtc,
                query.StartedToUtc,
                query.SubmittedFromUtc,
                query.SubmittedToUtc,
                query.Search),
            cancellationToken);

        return executions.Select(PreventiveExecutionMappings.ToSummaryDto).ToArray();
    }
}
