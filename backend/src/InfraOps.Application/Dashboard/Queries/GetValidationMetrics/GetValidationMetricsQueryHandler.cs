using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;

namespace InfraOps.Application.Dashboard.Queries.GetValidationMetrics;

public sealed class GetValidationMetricsQueryHandler
    : IQueryHandler<GetValidationMetricsQuery, ValidationMetricsDto>
{
    private readonly IValidator<GetValidationMetricsQuery> _validator;
    private readonly IDashboardMetricsRepository _dashboardMetricsRepository;

    public GetValidationMetricsQueryHandler(
        IValidator<GetValidationMetricsQuery> validator,
        IDashboardMetricsRepository dashboardMetricsRepository)
    {
        _validator = validator;
        _dashboardMetricsRepository = dashboardMetricsRepository;
    }

    public async Task<ValidationMetricsDto> Handle(
        GetValidationMetricsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        return await _dashboardMetricsRepository.GetValidationMetricsAsync(
            DashboardQueryMapping.ToFilter(query),
            cancellationToken);
    }
}
