using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Domain.PreventiveExecutions.Enums;

namespace InfraOps.Application.PreventiveValidations.Queries.ListPreventiveValidations;

public sealed class ListPreventiveValidationsQueryHandler
    : IQueryHandler<ListPreventiveValidationsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>>
{
    private readonly IValidator<ListPreventiveValidationsQuery> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;

    public ListPreventiveValidationsQueryHandler(
        IValidator<ListPreventiveValidationsQuery> validator,
        IPreventiveExecutionRepository preventiveExecutionRepository)
    {
        _validator = validator;
        _preventiveExecutionRepository = preventiveExecutionRepository;
    }

    public async Task<IReadOnlyCollection<PreventiveExecutionSummaryDto>> Handle(
        ListPreventiveValidationsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var status = PreventiveExecutionStatus.Submitted;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            PreventiveExecutionStatusCatalog.TryParse(query.Status, out status);
        }

        var executions = await _preventiveExecutionRepository.ListAsync(
            new PreventiveExecutionListFilter(
                status,
                query.EntityTypeId,
                query.InventoryItemId,
                query.SiteId,
                query.RegionId,
                null,
                query.SubmittedBy,
                null,
                null,
                query.SubmittedFromUtc,
                query.SubmittedToUtc,
                query.Search),
            cancellationToken);

        return executions.Select(PreventiveExecutionMappings.ToSummaryDto).ToArray();
    }
}
