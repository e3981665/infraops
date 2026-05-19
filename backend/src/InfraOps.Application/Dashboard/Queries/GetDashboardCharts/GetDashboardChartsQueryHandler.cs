using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetDashboardCharts;

public sealed class GetDashboardChartsQueryHandler
    : IQueryHandler<GetDashboardChartsQuery, DashboardChartsDto>
{
    private readonly IValidator<GetDashboardChartsQuery> _validator;
    private readonly IDashboardMetricsRepository _dashboardMetricsRepository;

    public GetDashboardChartsQueryHandler(
        IValidator<GetDashboardChartsQuery> validator,
        IDashboardMetricsRepository dashboardMetricsRepository)
    {
        _validator = validator;
        _dashboardMetricsRepository = dashboardMetricsRepository;
    }

    public async Task<DashboardChartsDto> Handle(
        GetDashboardChartsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _dashboardMetricsRepository.GetChartsAsync(
            DashboardQueryMapping.ToFilter(query),
            cancellationToken);
    }
}
