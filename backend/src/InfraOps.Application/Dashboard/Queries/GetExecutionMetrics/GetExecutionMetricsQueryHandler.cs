using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetExecutionMetrics;

public sealed class GetExecutionMetricsQueryHandler
    : IQueryHandler<GetExecutionMetricsQuery, ExecutionMetricsDto>
{
    private readonly IValidator<GetExecutionMetricsQuery> _validator;
    private readonly IDashboardMetricsRepository _dashboardMetricsRepository;

    public GetExecutionMetricsQueryHandler(
        IValidator<GetExecutionMetricsQuery> validator,
        IDashboardMetricsRepository dashboardMetricsRepository)
    {
        _validator = validator;
        _dashboardMetricsRepository = dashboardMetricsRepository;
    }

    public async Task<ExecutionMetricsDto> Handle(
        GetExecutionMetricsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _dashboardMetricsRepository.GetExecutionMetricsAsync(
            DashboardQueryMapping.ToFilter(query),
            cancellationToken);
    }
}
