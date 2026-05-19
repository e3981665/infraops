using InfraOps.Api.Contracts.Requests.PreventiveExecutions;
using InfraOps.Api.Contracts.Responses.PreventiveExecutions;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Commands.SavePreventiveExecutionDraft;
using InfraOps.Application.PreventiveExecutions.Commands.StartPreventiveExecution;
using InfraOps.Application.PreventiveExecutions.Commands.SubmitPreventiveExecution;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionById;
using InfraOps.Application.PreventiveExecutions.Queries.GetPreventiveExecutionFormDefinition;
using InfraOps.Application.PreventiveExecutions.Queries.ListPreventiveExecutions;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/preventive-executions")]
[Authorize]
public sealed class PreventiveExecutionsController : ControllerBase
{
    private readonly IQueryHandler<ListPreventiveExecutionsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>> _listHandler;
    private readonly IQueryHandler<GetPreventiveExecutionByIdQuery, PreventiveExecutionDetailsDto> _getByIdHandler;
    private readonly IQueryHandler<GetPreventiveExecutionFormDefinitionQuery, PreventiveExecutionFormDefinitionDto> _getFormDefinitionHandler;
    private readonly ICommandHandler<StartPreventiveExecutionCommand, PreventiveExecutionDetailsDto> _startHandler;
    private readonly ICommandHandler<SavePreventiveExecutionDraftCommand, PreventiveExecutionDetailsDto> _saveDraftHandler;
    private readonly ICommandHandler<SubmitPreventiveExecutionCommand, PreventiveExecutionDetailsDto> _submitHandler;
    private readonly ILogger<PreventiveExecutionsController> _logger;

    public PreventiveExecutionsController(
        IQueryHandler<ListPreventiveExecutionsQuery, IReadOnlyCollection<PreventiveExecutionSummaryDto>> listHandler,
        IQueryHandler<GetPreventiveExecutionByIdQuery, PreventiveExecutionDetailsDto> getByIdHandler,
        IQueryHandler<GetPreventiveExecutionFormDefinitionQuery, PreventiveExecutionFormDefinitionDto> getFormDefinitionHandler,
        ICommandHandler<StartPreventiveExecutionCommand, PreventiveExecutionDetailsDto> startHandler,
        ICommandHandler<SavePreventiveExecutionDraftCommand, PreventiveExecutionDetailsDto> saveDraftHandler,
        ICommandHandler<SubmitPreventiveExecutionCommand, PreventiveExecutionDetailsDto> submitHandler,
        ILogger<PreventiveExecutionsController> logger)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _getFormDefinitionHandler = getFormDefinitionHandler;
        _startHandler = startHandler;
        _saveDraftHandler = saveDraftHandler;
        _submitHandler = submitHandler;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.PreventiveRead)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PreventiveExecutionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PreventiveExecutionSummaryResponse>>> List(
        [FromQuery] string? status,
        [FromQuery] Guid? entityTypeId,
        [FromQuery] Guid? inventoryItemId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? regionId,
        [FromQuery] bool createdByCurrentUser,
        [FromQuery] DateTimeOffset? startedFromUtc,
        [FromQuery] DateTimeOffset? startedToUtc,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(
            new ListPreventiveExecutionsQuery(
                status,
                entityTypeId,
                inventoryItemId,
                siteId,
                regionId,
                createdByCurrentUser,
                startedFromUtc,
                startedToUtc,
                search),
            cancellationToken);

        return Ok(result.Select(MapSummaryResponse).ToArray());
    }

    [HttpGet("form-definition/{inventoryItemId:guid}")]
    [Authorize(Policy = PermissionCodes.PreventiveExecute)]
    [ProducesResponseType(typeof(PreventiveExecutionFormDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionFormDefinitionResponse>> GetFormDefinition(
        Guid inventoryItemId,
        CancellationToken cancellationToken)
    {
        var result = await _getFormDefinitionHandler.Handle(
            new GetPreventiveExecutionFormDefinitionQuery(inventoryItemId),
            cancellationToken);

        return Ok(MapFormDefinitionResponse(result));
    }

    [HttpGet("{preventiveExecutionId:guid}")]
    [Authorize(Policy = PermissionCodes.PreventiveRead)]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> GetById(
        Guid preventiveExecutionId,
        CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(
            new GetPreventiveExecutionByIdQuery(preventiveExecutionId),
            cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost("start")]
    [Authorize(Policy = PermissionCodes.PreventiveExecute)]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PreventiveExecutionResponse>> Start(
        [FromBody] StartPreventiveExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _startHandler.Handle(
            new StartPreventiveExecutionCommand(request.InventoryItemId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { preventiveExecutionId = result.Id }, MapResponse(result));
    }

    [HttpPut("{preventiveExecutionId:guid}/draft")]
    [Authorize(Policy = PermissionCodes.PreventiveExecute)]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> SaveDraft(
        Guid preventiveExecutionId,
        [FromBody] SavePreventiveExecutionDraftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _saveDraftHandler.Handle(
            new SavePreventiveExecutionDraftCommand(
                preventiveExecutionId,
                MapAnswers(request.Answers)),
            cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost("{preventiveExecutionId:guid}/submit")]
    [Authorize(Policy = PermissionCodes.PreventiveExecute)]
    [ProducesResponseType(typeof(PreventiveExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveExecutionResponse>> Submit(
        Guid preventiveExecutionId,
        [FromBody] SubmitPreventiveExecutionRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _submitHandler.Handle(
            new SubmitPreventiveExecutionCommand(
                preventiveExecutionId,
                request is null ? null : MapAnswers(request.Answers)),
            cancellationToken);

        _logger.LogInformation(
            "Preventive execution submitted {PreventiveExecutionId} for inventory item {InventoryItemId}.",
            result.Id,
            result.InventoryItemId);

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

    private static PreventiveExecutionFormDefinitionResponse MapFormDefinitionResponse(
        PreventiveExecutionFormDefinitionDto result)
    {
        return new PreventiveExecutionFormDefinitionResponse(
            result.InventoryItemId,
            result.InventoryItemDisplayName,
            result.EntityTypeId,
            result.EntityTypeName,
            result.EntityTypeCode,
            result.PreventiveTemplateId,
            result.PreventiveTemplateName,
            result.PreventiveTemplateCode,
            MapSections(result.Sections));
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

    private static IReadOnlyCollection<PreventiveExecutionAnswerInput> MapAnswers(
        IReadOnlyCollection<PreventiveExecutionAnswerRequest>? answers)
    {
        return (answers ?? [])
            .Select(x => new PreventiveExecutionAnswerInput(x.ItemKey, x.Value, x.Comment))
            .ToArray();
    }
}
