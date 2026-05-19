using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IValidator<GetDashboardSummaryQuery> _validator;
    private readonly IDashboardMetricsRepository _dashboardMetricsRepository;
    private readonly IClock _clock;

    public GetDashboardSummaryQueryHandler(
        IValidator<GetDashboardSummaryQuery> validator,
        IDashboardMetricsRepository dashboardMetricsRepository,
        IClock clock)
    {
        _validator = validator;
        _dashboardMetricsRepository = dashboardMetricsRepository;
        _clock = clock;
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _dashboardMetricsRepository.GetSummaryAsync(
            DashboardQueryMapping.ToFilter(query),
            _clock.UtcNow,
            cancellationToken);
    }
}
