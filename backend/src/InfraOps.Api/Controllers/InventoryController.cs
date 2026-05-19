using InfraOps.Api.Contracts.Requests.Inventory;
using InfraOps.Api.Contracts.Responses.Inventory;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Commands.ActivateInventoryItem;
using InfraOps.Application.Inventory.Commands.CreateInventoryItem;
using InfraOps.Application.Inventory.Commands.DeactivateInventoryItem;
using InfraOps.Application.Inventory.Commands.UpdateInventoryItem;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Queries.GetInventoryFormDefinitionByEntityType;
using InfraOps.Application.Inventory.Queries.GetInventoryFormMetadata;
using InfraOps.Application.Inventory.Queries.GetInventoryItemById;
using InfraOps.Application.Inventory.Queries.ListInventoryItems;
using InfraOps.Application.Inventory.Support;
using InfraOps.Domain.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraOps.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public sealed class InventoryController : ControllerBase
{
    private readonly IQueryHandler<ListInventoryItemsQuery, IReadOnlyCollection<InventoryItemSummaryDto>> _listHandler;
    private readonly IQueryHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto> _getByIdHandler;
    private readonly IQueryHandler<GetInventoryFormMetadataQuery, InventoryFormMetadataDto> _getFormMetadataHandler;
    private readonly IQueryHandler<GetInventoryFormDefinitionByEntityTypeQuery, InventoryFormDefinitionDto> _getFormDefinitionHandler;
    private readonly ICommandHandler<CreateInventoryItemCommand, InventoryItemDetailsDto> _createHandler;
    private readonly ICommandHandler<UpdateInventoryItemCommand, InventoryItemDetailsDto> _updateHandler;
    private readonly ICommandHandler<ActivateInventoryItemCommand> _activateHandler;
    private readonly ICommandHandler<DeactivateInventoryItemCommand> _deactivateHandler;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IQueryHandler<ListInventoryItemsQuery, IReadOnlyCollection<InventoryItemSummaryDto>> listHandler,
        IQueryHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto> getByIdHandler,
        IQueryHandler<GetInventoryFormMetadataQuery, InventoryFormMetadataDto> getFormMetadataHandler,
        IQueryHandler<GetInventoryFormDefinitionByEntityTypeQuery, InventoryFormDefinitionDto> getFormDefinitionHandler,
        ICommandHandler<CreateInventoryItemCommand, InventoryItemDetailsDto> createHandler,
        ICommandHandler<UpdateInventoryItemCommand, InventoryItemDetailsDto> updateHandler,
        ICommandHandler<ActivateInventoryItemCommand> activateHandler,
        ICommandHandler<DeactivateInventoryItemCommand> deactivateHandler,
        ILogger<InventoryController> logger)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _getFormMetadataHandler = getFormMetadataHandler;
        _getFormDefinitionHandler = getFormDefinitionHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _activateHandler = activateHandler;
        _deactivateHandler = deactivateHandler;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.InventoryRead)]
    [ProducesResponseType(typeof(IReadOnlyCollection<InventoryItemSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemSummaryResponse>>> List(
        [FromQuery] Guid? entityTypeId,
        [FromQuery] string? status,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? regionId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _listHandler.Handle(
            new ListInventoryItemsQuery(entityTypeId, status, siteId, regionId, isActive, search),
            cancellationToken);

        return Ok(result.Select(MapSummaryResponse).ToArray());
    }

    [HttpGet("form-metadata")]
    [Authorize(Policy = PermissionCodes.InventoryRead)]
    [ProducesResponseType(typeof(InventoryFormMetadataResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryFormMetadataResponse>> GetFormMetadata(CancellationToken cancellationToken)
    {
        var result = await _getFormMetadataHandler.Handle(new GetInventoryFormMetadataQuery(), cancellationToken);

        return Ok(MapFormMetadataResponse(result));
    }

    [HttpGet("form-definition/{entityTypeId:guid}")]
    [Authorize(Policy = PermissionCodes.InventoryRead)]
    [ProducesResponseType(typeof(InventoryFormDefinitionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryFormDefinitionResponse>> GetFormDefinition(
        Guid entityTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _getFormDefinitionHandler.Handle(
            new GetInventoryFormDefinitionByEntityTypeQuery(entityTypeId),
            cancellationToken);

        return Ok(MapFormDefinitionResponse(result));
    }

    [HttpGet("{inventoryItemId:guid}")]
    [Authorize(Policy = PermissionCodes.InventoryRead)]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryItemResponse>> GetById(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.Handle(new GetInventoryItemByIdQuery(inventoryItemId), cancellationToken);

        return Ok(MapResponse(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.InventoryWrite)]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryItemResponse>> Create(
        [FromBody] CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.Handle(
            new CreateInventoryItemCommand(
                request.EntityTypeId,
                request.RegionId,
                request.SiteId,
                request.DisplayName,
                request.Status,
                request.InstallationDate,
                MapAttributeValues(request.AttributeValues)),
            cancellationToken);

        _logger.LogInformation(
            "Inventory item created {InventoryItemId} for entity type {EntityTypeId} at site {SiteId}.",
            result.Id,
            result.EntityTypeId,
            result.SiteId);

        return CreatedAtAction(nameof(GetById), new { inventoryItemId = result.Id }, MapResponse(result));
    }

    [HttpPut("{inventoryItemId:guid}")]
    [Authorize(Policy = PermissionCodes.InventoryWrite)]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryItemResponse>> Update(
        Guid inventoryItemId,
        [FromBody] UpdateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateHandler.Handle(
            new UpdateInventoryItemCommand(
                inventoryItemId,
                request.RegionId,
                request.SiteId,
                request.DisplayName,
                request.Status,
                request.InstallationDate,
                MapAttributeValues(request.AttributeValues)),
            cancellationToken);

        _logger.LogInformation(
            "Inventory item updated {InventoryItemId} at site {SiteId}.",
            result.Id,
            result.SiteId);

        return Ok(MapResponse(result));
    }

    [HttpPost("{inventoryItemId:guid}/activate")]
    [Authorize(Policy = PermissionCodes.InventoryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        await _activateHandler.Handle(new ActivateInventoryItemCommand(inventoryItemId), cancellationToken);

        _logger.LogInformation("Inventory item activated {InventoryItemId}.", inventoryItemId);

        return NoContent();
    }

    [HttpPost("{inventoryItemId:guid}/deactivate")]
    [Authorize(Policy = PermissionCodes.InventoryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        await _deactivateHandler.Handle(new DeactivateInventoryItemCommand(inventoryItemId), cancellationToken);

        _logger.LogInformation("Inventory item deactivated {InventoryItemId}.", inventoryItemId);

        return NoContent();
    }

    private static InventoryItemResponse MapResponse(InventoryItemDetailsDto result)
    {
        return new InventoryItemResponse(
            result.Id,
            result.EntityTypeId,
            result.EntityTypeName,
            result.EntityTypeCode,
            result.RegionId,
            result.RegionName,
            result.SiteId,
            result.SiteName,
            result.DisplayName,
            result.Status,
            result.InstallationDate,
            result.IsActive,
            result.CreatedBy,
            result.UpdatedBy,
            result.CreatedAtUtc,
            result.UpdatedAtUtc,
            result.AttributeValues
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new InventoryAttributeValueResponse(
                    x.EntityFieldDefinitionId,
                    x.FieldKey,
                    x.DisplayLabel,
                    x.FieldType,
                    x.DisplayOrder,
                    x.Value))
                .ToArray());
    }

    private static InventoryItemSummaryResponse MapSummaryResponse(InventoryItemSummaryDto result)
    {
        return new InventoryItemSummaryResponse(
            result.Id,
            result.EntityTypeId,
            result.EntityTypeName,
            result.RegionId,
            result.RegionName,
            result.SiteId,
            result.SiteName,
            result.DisplayName,
            result.Status,
            result.InstallationDate,
            result.IsActive);
    }

    private static InventoryFormMetadataResponse MapFormMetadataResponse(InventoryFormMetadataDto result)
    {
        return new InventoryFormMetadataResponse(
            result.EntityTypes.Select(x => new InventoryLookupOptionResponse(x.Id, x.Code, x.Name)).ToArray(),
            result.Regions.Select(x => new InventoryLookupOptionResponse(x.Id, x.Code, x.Name)).ToArray(),
            result.Sites.Select(x => new InventorySiteOptionResponse(x.Id, x.RegionId, x.Code, x.Name)).ToArray(),
            result.Statuses.Select(x => new InventoryStatusOptionResponse(x.Code, x.Label)).ToArray());
    }

    private static InventoryFormDefinitionResponse MapFormDefinitionResponse(InventoryFormDefinitionDto result)
    {
        return new InventoryFormDefinitionResponse(
            result.EntityTypeId,
            result.EntityTypeName,
            result.EntityTypeCode,
            result.FieldDefinitions
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new InventoryFormFieldDefinitionResponse(
                    x.Id,
                    x.FieldKey,
                    x.DisplayLabel,
                    x.FieldType,
                    x.DisplayOrder,
                    x.IsRequired,
                    x.Placeholder,
                    x.HelpText,
                    x.Options
                        .OrderBy(option => option.DisplayOrder)
                        .Select(option => new InventoryFormFieldOptionResponse(
                            option.Id,
                            option.Value,
                            option.Label,
                            option.DisplayOrder))
                        .ToArray()))
                .ToArray());
    }

    private static IReadOnlyCollection<InventoryAttributeValueInput> MapAttributeValues(
        IReadOnlyCollection<InventoryAttributeValueRequest>? attributeValues)
    {
        return (attributeValues ?? [])
            .Select(x => new InventoryAttributeValueInput(x.FieldKey, x.Value))
            .ToArray();
    }
}
