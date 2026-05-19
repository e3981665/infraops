using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetInventoryMetrics;

public sealed class GetInventoryMetricsQueryHandler
    : IQueryHandler<GetInventoryMetricsQuery, InventoryMetricsDto>
{
    private readonly IValidator<GetInventoryMetricsQuery> _validator;
    private readonly IDashboardMetricsRepository _dashboardMetricsRepository;

    public GetInventoryMetricsQueryHandler(
        IValidator<GetInventoryMetricsQuery> validator,
        IDashboardMetricsRepository dashboardMetricsRepository)
    {
        _validator = validator;
        _dashboardMetricsRepository = dashboardMetricsRepository;
    }

    public async Task<InventoryMetricsDto> Handle(
        GetInventoryMetricsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _dashboardMetricsRepository.GetInventoryMetricsAsync(
            DashboardQueryMapping.ToFilter(query),
            cancellationToken);
    }
}
