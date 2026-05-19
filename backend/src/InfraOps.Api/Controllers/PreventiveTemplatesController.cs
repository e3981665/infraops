using InfraOps.Api.Contracts.Requests.PreventiveTemplates;
using InfraOps.Api.Contracts.Responses.PreventiveTemplates;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveTemplates.Commands.ActivatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Commands.CreatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Commands.DeactivatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Commands.UpdatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateById;
using InfraOps.Application.PreventiveTemplates.Queries.GetPreventiveTemplateFormMetadata;
using InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplates;
using InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplatesByEntityType;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/preventive-templates")]
[Authorize]
public sealed class PreventiveTemplatesController : ControllerBase
{
    private readonly IQueryHandler<ListPreventiveTemplatesQuery, IReadOnlyCollection<PreventiveTemplateSummaryDto>> _listHandler;
    private readonly IQueryHandler<GetPreventiveTemplateByIdQuery, PreventiveTemplateDetailsDto> _getByIdHandler;
    private readonly IQueryHandler<ListPreventiveTemplatesByEntityTypeQuery, IReadOnlyCollection<PreventiveTemplateDetailsDto>> _listByEntityTypeHandler;
    private readonly IQueryHandler<GetPreventiveTemplateFormMetadataQuery, PreventiveTemplateFormMetadataDto> _getFormMetadataHandler;
    private readonly ICommandHandler<CreatePreventiveTemplateCommand, PreventiveTemplateDetailsDto> _createHandler;
    private readonly ICommandHandler<UpdatePreventiveTemplateCommand, PreventiveTemplateDetailsDto> _updateHandler;
    private readonly ICommandHandler<ActivatePreventiveTemplateCommand> _activateHandler;
    private readonly ICommandHandler<DeactivatePreventiveTemplateCommand> _deactivateHandler;

    public PreventiveTemplatesController(
        IQueryHandler<ListPreventiveTemplatesQuery, IReadOnlyCollection<PreventiveTemplateSummaryDto>> listHandler,
        IQueryHandler<GetPreventiveTemplateByIdQuery, PreventiveTemplateDetailsDto> getByIdHandler,
        IQueryHandler<ListPreventiveTemplatesByEntityTypeQuery, IReadOnlyCollection<PreventiveTemplateDetailsDto>> listByEntityTypeHandler,
        IQueryHandler<GetPreventiveTemplateFormMetadataQuery, PreventiveTemplateFormMetadataDto> getFormMetadataHandler,
        ICommandHandler<CreatePreventiveTemplateCommand, PreventiveTemplateDetailsDto> createHandler,
        ICommandHandler<UpdatePreventiveTemplateCommand, PreventiveTemplateDetailsDto> updateHandler,
        ICommandHandler<ActivatePreventiveTemplateCommand> activateHandler,
        ICommandHandler<DeactivatePreventiveTemplateCommand> deactivateHandler)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _listByEntityTypeHandler = listByEntityTypeHandler;
        _getFormMetadataHandler = getFormMetadataHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _activateHandler = activateHandler;
        _deactivateHandler = deactivateHandler;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesRead)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PreventiveTemplateSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PreventiveTemplateSummaryResponse>>> List(
        [FromQuery] Guid? entityTypeId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(
            new ListPreventiveTemplatesQuery(entityTypeId, isActive, search),
            cancellationToken);

        return Ok(result.Select(MapSummaryResponse).ToArray());
    }

    [HttpGet("form-metadata")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesRead)]
    [ProducesResponseType(typeof(PreventiveTemplateFormMetadataResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveTemplateFormMetadataResponse>> GetFormMetadata(CancellationToken cancellationToken)
    {
        var result = await _getFormMetadataHandler.Handle(new GetPreventiveTemplateFormMetadataQuery(), cancellationToken);

        return Ok(new PreventiveTemplateFormMetadataResponse(
            result.EntityTypes
                .Select(x => new PreventiveTemplateEntityTypeOptionResponse(x.Id, x.Code, x.Name))
                .ToArray()));
    }

    [HttpGet("by-entity-type/{entityTypeId:guid}")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesRead)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PreventiveTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PreventiveTemplateResponse>>> ListByEntityType(
        Guid entityTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _listByEntityTypeHandler.Handle(
            new ListPreventiveTemplatesByEntityTypeQuery(entityTypeId),
            cancellationToken);

        return Ok(result.Select(MapResponse).ToArray());
    }

    [HttpGet("{preventiveTemplateId:guid}")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesRead)]
    [ProducesResponseType(typeof(PreventiveTemplateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveTemplateResponse>> GetById(
        Guid preventiveTemplateId,
        CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(new GetPreventiveTemplateByIdQuery(preventiveTemplateId), cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesWrite)]
    [ProducesResponseType(typeof(PreventiveTemplateResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PreventiveTemplateResponse>> Create(
        [FromBody] CreatePreventiveTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.Handle(
            new CreatePreventiveTemplateCommand(
                request.EntityTypeId,
                request.Name,
                request.Code,
                request.Description,
                MapSections(request.Sections)),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { preventiveTemplateId = result.Id }, MapResponse(result));
    }

    [HttpPut("{preventiveTemplateId:guid}")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesWrite)]
    [ProducesResponseType(typeof(PreventiveTemplateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PreventiveTemplateResponse>> Update(
        Guid preventiveTemplateId,
        [FromBody] UpdatePreventiveTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateHandler.Handle(
            new UpdatePreventiveTemplateCommand(
                preventiveTemplateId,
                request.Name,
                request.Code,
                request.Description,
                MapSections(request.Sections)),
            cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost("{preventiveTemplateId:guid}/activate")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(Guid preventiveTemplateId, CancellationToken cancellationToken)
    {
        await _activateHandler.Handle(new ActivatePreventiveTemplateCommand(preventiveTemplateId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{preventiveTemplateId:guid}/deactivate")]
    [Authorize(Policy = PermissionCodes.PreventiveTemplatesWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid preventiveTemplateId, CancellationToken cancellationToken)
    {
        await _deactivateHandler.Handle(new DeactivatePreventiveTemplateCommand(preventiveTemplateId), cancellationToken);

        return NoContent();
    }

    private static PreventiveTemplateResponse MapResponse(PreventiveTemplateDetailsDto result)
    {
        return new PreventiveTemplateResponse(
            result.Id,
            result.EntityTypeId,
            result.EntityTypeName,
            result.EntityTypeCode,
            result.Name,
            result.Code,
            result.Description,
            result.IsActive,
            result.Sections
                .OrderBy(x => x.DisplayOrder)
                .Select(section => new PreventiveTemplateSectionResponse(
                    section.Id,
                    section.Title,
                    section.DisplayOrder,
                    section.IsActive,
                    section.ChecklistItems
                        .OrderBy(x => x.DisplayOrder)
                        .Select(item => new PreventiveChecklistItemResponse(
                            item.Id,
                            item.ItemKey,
                            item.Label,
                            item.ItemType,
                            item.DisplayOrder,
                            item.IsRequired,
                            item.IsActive,
                            item.HelpText,
                            item.IsCritical,
                            item.RequiresCommentOnFailure,
                            item.RequiresPhotoOnFailure,
                            item.MinimumValue,
                            item.MaximumValue,
                            item.Options
                                .OrderBy(x => x.DisplayOrder)
                                .Select(option => new PreventiveChecklistOptionResponse(
                                    option.Id,
                                    option.Value,
                                    option.Label,
                                    option.DisplayOrder))
                                .ToArray()))
                        .ToArray()))
                .ToArray());
    }

    private static PreventiveTemplateSummaryResponse MapSummaryResponse(PreventiveTemplateSummaryDto result)
    {
        return new PreventiveTemplateSummaryResponse(
            result.Id,
            result.EntityTypeId,
            result.EntityTypeName,
            result.Name,
            result.Code,
            result.Description,
            result.IsActive,
            result.SectionCount,
            result.ChecklistItemCount);
    }

    private static IReadOnlyCollection<PreventiveTemplateSectionInput> MapSections(
        IReadOnlyCollection<PreventiveTemplateSectionRequest>? sections)
    {
        return (sections ?? [])
            .Select(section => new PreventiveTemplateSectionInput(
                section.Id,
                section.Title,
                section.DisplayOrder,
                section.IsActive,
                (section.ChecklistItems ?? [])
                    .Select(item => new PreventiveChecklistItemInput(
                        item.Id,
                        item.ItemKey,
                        item.Label,
                        item.ItemType,
                        item.DisplayOrder,
                        item.IsRequired,
                        item.IsActive,
                        item.HelpText,
                        item.IsCritical,
                        item.RequiresCommentOnFailure,
                        item.RequiresPhotoOnFailure,
                        item.MinimumValue,
                        item.MaximumValue,
                        (item.Options ?? [])
                            .Select(option => new PreventiveChecklistOptionInput(
                                option.Id,
                                option.Value,
                                option.Label,
                                option.DisplayOrder))
                            .ToArray()))
                    .ToArray()))
            .ToArray();
    }
}
