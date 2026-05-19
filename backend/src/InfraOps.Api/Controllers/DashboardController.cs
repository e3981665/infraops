using InfraOps.Api.Contracts.Responses.Dashboard;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Dashboard.Dtos;
using InfraOps.Application.Dashboard.Queries.GetDashboardCharts;
using InfraOps.Application.Dashboard.Queries.GetDashboardSummary;
using InfraOps.Application.Dashboard.Queries.GetExecutionMetrics;
using InfraOps.Application.Dashboard.Queries.GetInventoryMetrics;
using InfraOps.Application.Dashboard.Queries.GetValidationMetrics;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = PermissionCodes.PreventiveRead)]
public sealed class DashboardController : ControllerBase
{
    private readonly IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto> _summaryHandler;
    private readonly IQueryHandler<GetExecutionMetricsQuery, ExecutionMetricsDto> _executionMetricsHandler;
    private readonly IQueryHandler<GetValidationMetricsQuery, ValidationMetricsDto> _validationMetricsHandler;
    private readonly IQueryHandler<GetInventoryMetricsQuery, InventoryMetricsDto> _inventoryMetricsHandler;
    private readonly IQueryHandler<GetDashboardChartsQuery, DashboardChartsDto> _chartsHandler;

    public DashboardController(
        IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto> summaryHandler,
        IQueryHandler<GetExecutionMetricsQuery, ExecutionMetricsDto> executionMetricsHandler,
        IQueryHandler<GetValidationMetricsQuery, ValidationMetricsDto> validationMetricsHandler,
        IQueryHandler<GetInventoryMetricsQuery, InventoryMetricsDto> inventoryMetricsHandler,
        IQueryHandler<GetDashboardChartsQuery, DashboardChartsDto> chartsHandler)
    {
        _summaryHandler = summaryHandler;
        _executionMetricsHandler = executionMetricsHandler;
        _validationMetricsHandler = validationMetricsHandler;
        _inventoryMetricsHandler = inventoryMetricsHandler;
        _chartsHandler = chartsHandler;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _summaryHandler.Handle(
            new GetDashboardSummaryQuery(regionId, siteId, entityTypeId, fromUtc, toUtc),
            cancellationToken);

        return Ok(new DashboardSummaryResponse(
            result.TotalInventoryItems,
            result.ActiveInventoryItems,
            result.PreventiveExecutionsThisMonth,
            result.PendingValidationExecutions,
            result.ApprovedPreventiveExecutions,
            result.RejectedPreventiveExecutions,
            result.ReworkRequestedPreventiveExecutions,
            result.ActiveEntityTypes,
            result.ActivePreventiveTemplates));
    }

    [HttpGet("executions")]
    [ProducesResponseType(typeof(ExecutionMetricsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExecutionMetricsResponse>> GetExecutions(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _executionMetricsHandler.Handle(
            new GetExecutionMetricsQuery(regionId, siteId, entityTypeId, fromUtc, toUtc),
            cancellationToken);

        return Ok(new ExecutionMetricsResponse(
            result.TotalExecutions,
            result.DraftExecutions,
            result.SubmittedExecutions,
            result.ApprovedExecutions,
            result.RejectedExecutions,
            result.ReworkRequestedExecutions,
            MapPoints(result.ExecutionsByEntityType)));
    }

    [HttpGet("validations")]
    [ProducesResponseType(typeof(ValidationMetricsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidationMetricsResponse>> GetValidations(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _validationMetricsHandler.Handle(
            new GetValidationMetricsQuery(regionId, siteId, entityTypeId, fromUtc, toUtc),
            cancellationToken);

        return Ok(new ValidationMetricsResponse(
            result.PendingValidation,
            result.Approved,
            result.Rejected,
            result.ReworkRequested,
            MapPoints(result.ResultsByStatus)));
    }

    [HttpGet("inventory")]
    [ProducesResponseType(typeof(InventoryMetricsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryMetricsResponse>> GetInventory(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryMetricsHandler.Handle(
            new GetInventoryMetricsQuery(regionId, siteId, entityTypeId, fromUtc, toUtc),
            cancellationToken);

        return Ok(new InventoryMetricsResponse(
            result.TotalInventoryItems,
            result.ActiveInventoryItems,
            MapPoints(result.InventoryByEntityType)));
    }

    [HttpGet("charts")]
    [ProducesResponseType(typeof(DashboardChartsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardChartsResponse>> GetCharts(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _chartsHandler.Handle(
            new GetDashboardChartsQuery(regionId, siteId, entityTypeId, fromUtc, toUtc),
            cancellationToken);

        return Ok(new DashboardChartsResponse(
            MapPoints(result.ExecutionsByMonth),
            MapPoints(result.ValidationResultsByStatus),
            MapPoints(result.InventoryByEntityType),
            MapPoints(result.ExecutionsByEntityType)));
    }

    private static IReadOnlyCollection<DashboardMetricPointResponse> MapPoints(
        IReadOnlyCollection<DashboardMetricPointDto> points)
    {
        return points
            .Select(x => new DashboardMetricPointResponse(x.Label, x.Value))
            .ToArray();
    }
}
