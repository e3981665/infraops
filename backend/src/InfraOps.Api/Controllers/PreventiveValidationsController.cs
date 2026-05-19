using InfraOps.Api.Contracts.Requests.PreventiveValidations;
using InfraOps.Api.Contracts.Responses.PreventiveExecutions;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveValidations.Commands.ApprovePreventiveExecution;
using InfraOps.Application.PreventiveValidations.Commands.RejectPreventiveExecution;
using InfraOps.Application.PreventiveValidations.Commands.RequestPreventiveRework;
using InfraOps.Application.PreventiveValidations.Queries.GetPreventiveValidationDetail;
using InfraOps.Application.PreventiveValidations.Queries.ListPreventiveValidations;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/preventive-validations")]
[Authorize(Policy = PermissionCodes.PreventiveValidate)]
public sealed class PreventiveValidationsController : ControllerBase
{
    private readonly IQueryHandler<ListPreventiveValidationsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>> _listHandler;
    private readonly IQueryHandler<GetPreventiveValidationDetailQuery, PreventiveExecutionDetailsDto> _detailHandler;
    private readonly ICommandHandler<ApprovePreventiveExecutionCommand, PreventiveExecutionDetailsDto> _approveHandler;
    private readonly ICommandHandler<RejectPreventiveExecutionCommand, PreventiveExecutionDetailsDto> _rejectHandler;
    private readonly ICommandHandler<RequestPreventiveReworkCommand, PreventiveExecutionDetailsDto> _requestReworkHandler;
    private readonly ILogger<PreventiveValidationsController> _logger;

    public PreventiveValidationsController(
        IQueryHandler<ListPreventiveValidationsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>> listHandler,
        IQueryHandler<GetPreventiveValidationDetailQuery, PreventiveExecutionDetailsDto> detailHandler,
        ICommandHandler<ApprovePreventiveExecutionCommand, PreventiveExecutionDetailsDto> approveHandler,
        ICommandHandler<RejectPreventiveExecutionCommand, PreventiveExecutionDetailsDto> rejectHandler,
        ICommandHandler<RequestPreventiveReworkCommand, PreventiveExecutionDetailsDto> requestReworkHandler,
        ILogger<PreventiveValidationsController> logger)
    {
        _listHandler = listHandler;
        _detailHandler = detailHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _requestReworkHandler = requestReworkHandler;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PreventiveExecutionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PreventiveExecutionSummaryResponse>>> List(
        [FromQuery] string? status,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] Guid? inventoryItemId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? submittedBy,
        [FromQuery] DateTimeOffset? submittedFromUtc,
        [FromQuery] DateTimeOffset? submittedToUtc,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(
            new ListPreventiveValidationsQuery(
                status,
                entityTypeId,
                inventoryItemId,
                siteId,
                regionId,
                submittedBy,
                submittedFromUtc,
                submittedToUtc,
                search),
            cancellationToken);

        return Ok(result.Select(MapSummaryResponse).ToArray());
    }

    [HttpGet("{preventiveExecutionId:guid}")]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> GetDetail(
        Guid preventiveExecutionId,
        CancellationToken cancellationToken)
    {
        var result = await _detailHandler.Handle(
            new GetPreventiveValidationDetailQuery(preventiveExecutionId),
            cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost("{preventiveExecutionId:guid}/approve")]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> Approve(
        Guid preventiveExecutionId,
        [FromBody] ApprovePreventiveExecutionRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _approveHandler.Handle(
            new ApprovePreventiveExecutionCommand(preventiveExecutionId, request?.Comment),
            cancellationToken);

        _logger.LogInformation(
            "Preventive execution approved {PreventiveExecutionId}.",
            result.Id);

        return Ok(MapResponse(result));
    }

    [HttpPost("{preventiveExecutionId:guid}/reject")]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> Reject(
        Guid preventiveExecutionId,
        [FromBody] RejectPreventiveExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rejectHandler.Handle(
            new RejectPreventiveExecutionCommand(preventiveExecutionId, request.Reason),
            cancellationToken);

        _logger.LogInformation(
            "Preventive execution rejected {PreventiveExecutionId}.",
            result.Id);

        return Ok(MapResponse(result));
    }

    [HttpPost("{preventiveExecutionId:guid}/request-rework")]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> RequestRework(
        Guid preventiveExecutionId,
        [FromBody] RequestPreventiveReworkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _requestReworkHandler.Handle(
            new RequestPreventiveReworkCommand(preventiveExecutionId, request.Reason),
            cancellationToken);

        _logger.LogInformation(
            "Preventive execution rework requested {PreventiveExecutionId}.",
            result.Id);

        return Ok(MapResponse(result));
    }

    private static PreventiveExecutionResponse MapResponse(PreventiveExecutionDetailsDto result)
    {
        return new PreventiveExecutionResponse(
            result.Id,
            result.InventoryItemId,
            result.InventoryItemDisplayName,
            result.PreventiveTemplateId,
            result.PreventiveTemplateName,
            result.PreventiveTemplateCode,
            result.EntityTypeId,
            result.EntityTypeName,
            result.EntityTypeCode,
            result.RegionId,
            result.RegionName,
            result.SiteId,
            result.SiteName,
            result.Status,
            result.CreatedBy,
            result.UpdatedBy,
            result.SubmittedBy,
            result.CreatedAtUtc,
            result.UpdatedAtUtc,
            result.SubmittedAtUtc,
            MapSections(result.TemplateSections),
            result.Answers
                .Select(answer => new PreventiveExecutionAnswerResponse(
                    answer.Id,
                    answer.ItemKey,
                    answer.Value,
                    answer.Comment))
                .ToArray(),
            result.ValidationHistory
                .Select(record => new PreventiveValidationRecordResponse(
                    record.Id,
                    record.ActionType,
                    record.ValidatorUserId,
                    record.CreatedAtUtc,
                    record.Comment))
                .ToArray());
    }

    private static PreventiveExecutionSummaryResponse MapSummaryResponse(PreventiveExecutionSummaryDto result)
    {
        return new PreventiveExecutionSummaryResponse(
            result.Id,
            result.InventoryItemId,
            result.InventoryItemDisplayName,
            result.PreventiveTemplateId,
            result.PreventiveTemplateName,
            result.EntityTypeId,
            result.EntityTypeName,
            result.RegionId,
            result.RegionName,
            result.SiteId,
            result.SiteName,
            result.Status,
            result.CreatedBy,
            result.UpdatedBy,
            result.SubmittedBy,
            result.CreatedAtUtc,
            result.UpdatedAtUtc,
            result.SubmittedAtUtc);
    }

    private static IReadOnlyCollection<PreventiveExecutionTemplateSectionResponse> MapSections(
        IReadOnlyCollection<PreventiveExecutionTemplateSectionDto> sections)
    {
        return sections
            .OrderBy(x => x.DisplayOrder)
            .Select(section => new PreventiveExecutionTemplateSectionResponse(
                section.Id,
                section.SourceTemplateSectionId,
                section.Title,
                section.DisplayOrder,
                section.ChecklistItems
                    .OrderBy(x => x.DisplayOrder)
                    .Select(item => new PreventiveExecutionTemplateItemResponse(
                        item.Id,
                        item.SourceChecklistItemId,
                        item.ItemKey,
                        item.Label,
                        item.ItemType,
                        item.DisplayOrder,
                        item.IsRequired,
                        item.HelpText,
                        item.IsCritical,
                        item.RequiresCommentOnFailure,
                        item.RequiresPhotoOnFailure,
                        item.MinimumValue,
                        item.MaximumValue,
                        item.Options
                            .OrderBy(option => option.DisplayOrder)
                            .Select(option => new PreventiveExecutionOptionResponse(
                                option.Id,
                                option.Value,
                                option.Label,
                                option.DisplayOrder))
                            .ToArray()))
                    .ToArray()))
            .ToArray();
    }
}
