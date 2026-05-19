using InfraOps.Api.Contracts.Requests.EntityTypes;
using InfraOps.Api.Contracts.Responses.EntityTypes;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.EntityTypes.Commands.ActivateEntityType;
using InfraOps.Application.EntityTypes.Commands.CreateEntityType;
using InfraOps.Application.EntityTypes.Commands.DeactivateEntityType;
using InfraOps.Application.EntityTypes.Commands.UpdateEntityType;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Queries.GetEntityTypeById;
using InfraOps.Application.EntityTypes.Queries.ListEntityTypes;
using InfraOps.Application.EntityTypes.Support;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/entity-types")]
[Authorize(Policy = PermissionCodes.EntityManage)]
public sealed class EntityTypesController : ControllerBase
{
    private readonly IQueryHandler<ListEntityTypesQuery, IReadOnlyCollection<EntityTypeSummaryDto>> _listHandler;
    private readonly IQueryHandler<GetEntityTypeByIdQuery, EntityTypeDetailsDto> _getByIdHandler;
    private readonly ICommandHandler<CreateEntityTypeCommand, EntityTypeDetailsDto> _createHandler;
    private readonly ICommandHandler<UpdateEntityTypeCommand, EntityTypeDetailsDto> _updateHandler;
    private readonly ICommandHandler<ActivateEntityTypeCommand> _activateHandler;
    private readonly ICommandHandler<DeactivateEntityTypeCommand> _deactivateHandler;
    private readonly ILogger<EntityTypesController> _logger;

    public EntityTypesController(
        IQueryHandler<ListEntityTypesQuery, IReadOnlyCollection<EntityTypeSummaryDto>> listHandler,
        IQueryHandler<GetEntityTypeByIdQuery, EntityTypeDetailsDto> getByIdHandler,
        ICommandHandler<CreateEntityTypeCommand, EntityTypeDetailsDto> createHandler,
        ICommandHandler<UpdateEntityTypeCommand, EntityTypeDetailsDto> updateHandler,
        ICommandHandler<ActivateEntityTypeCommand> activateHandler,
        ICommandHandler<DeactivateEntityTypeCommand> deactivateHandler,
        ILogger<EntityTypesController> logger)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _activateHandler = activateHandler;
        _deactivateHandler = deactivateHandler;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<EntityTypeSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<EntityTypeSummaryResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(new ListEntityTypesQuery(), cancellationToken);

        return Ok(result.Select(MapSummaryResponse).ToArray());
    }

    [HttpGet("{entityTypeId:guid}")]
    [ProducesResponseType(typeof(EntityTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EntityTypeResponse>> GetById(Guid entityTypeId, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(new GetEntityTypeByIdQuery(entityTypeId), cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EntityTypeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EntityTypeResponse>> Create(
        [FromBody] CreateEntityTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.Handle(
            new CreateEntityTypeCommand(
                request.Name,
                request.Code,
                request.Description,
                MapFieldDefinitions(request.FieldDefinitions)),
            cancellationToken);

        _logger.LogInformation(
            "Entity type created {EntityTypeId} with code {EntityTypeCode}.",
            result.Id,
            result.Code);

        return CreatedAtAction(nameof(GetById), new { entityTypeId = result.Id }, MapResponse(result));
    }

    [HttpPut("{entityTypeId:guid}")]
    [ProducesResponseType(typeof(EntityTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EntityTypeResponse>> Update(
        Guid entityTypeId,
        [FromBody] UpdateEntityTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateHandler.Handle(
            new UpdateEntityTypeCommand(
                entityTypeId,
                request.Name,
                request.Code,
                request.Description,
                MapFieldDefinitions(request.FieldDefinitions)),
            cancellationToken);

        _logger.LogInformation(
            "Entity type updated {EntityTypeId} with code {EntityTypeCode}.",
            result.Id,
            result.Code);

        return Ok(MapResponse(result));
    }

    [HttpPost("{entityTypeId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(Guid entityTypeId, CancellationToken cancellationToken)
    {
        await _activateHandler.Handle(new ActivateEntityTypeCommand(entityTypeId), cancellationToken);

        _logger.LogInformation("Entity type activated {EntityTypeId}.", entityTypeId);

        return NoContent();
    }

    [HttpPost("{entityTypeId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid entityTypeId, CancellationToken cancellationToken)
    {
        await _deactivateHandler.Handle(new DeactivateEntityTypeCommand(entityTypeId), cancellationToken);

        _logger.LogInformation("Entity type deactivated {EntityTypeId}.", entityTypeId);

        return NoContent();
    }

    private static EntityTypeResponse MapResponse(EntityTypeDetailsDto result)
    {
        return new EntityTypeResponse(
            result.Id,
            result.Name,
            result.Code,
            result.Description,
            result.IsActive,
            result.FieldDefinitions
                .OrderBy(x => x.DisplayOrder)
                .Select(fieldDefinition => new EntityFieldDefinitionResponse(
                    fieldDefinition.Id,
                    fieldDefinition.FieldKey,
                    fieldDefinition.DisplayLabel,
                    fieldDefinition.FieldType,
                    fieldDefinition.DisplayOrder,
                    fieldDefinition.IsRequired,
                    fieldDefinition.IsActive,
                    fieldDefinition.Placeholder,
                    fieldDefinition.HelpText,
                    fieldDefinition.Options
                        .OrderBy(x => x.DisplayOrder)
                        .Select(option => new EntityFieldOptionResponse(
                            option.Id,
                            option.Value,
                            option.Label,
                            option.DisplayOrder))
                        .ToArray()))
                .ToArray());
    }

    private static EntityTypeSummaryResponse MapSummaryResponse(EntityTypeSummaryDto result)
    {
        return new EntityTypeSummaryResponse(
            result.Id,
            result.Name,
            result.Code,
            result.Description,
            result.IsActive,
            result.FieldCount);
    }

    private static IReadOnlyCollection<EntityFieldDefinitionInput> MapFieldDefinitions(
        IReadOnlyCollection<EntityFieldDefinitionRequest>? fieldDefinitions)
    {
        return (fieldDefinitions ?? [])
            .Select(fieldDefinition => new EntityFieldDefinitionInput(
                fieldDefinition.Id,
                fieldDefinition.FieldKey,
                fieldDefinition.DisplayLabel,
                fieldDefinition.FieldType,
                fieldDefinition.DisplayOrder,
                fieldDefinition.IsRequired,
                fieldDefinition.IsActive,
                fieldDefinition.Placeholder,
                fieldDefinition.HelpText,
                (fieldDefinition.Options ?? [])
                    .Select(option => new EntityFieldOptionInput(
                        option.Id,
                        option.Value,
                        option.Label,
                        option.DisplayOrder))
                    .ToArray()))
            .ToArray();
    }
}
