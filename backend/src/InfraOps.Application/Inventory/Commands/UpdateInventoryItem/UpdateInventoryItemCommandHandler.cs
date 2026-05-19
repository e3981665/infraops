using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;
using InfraOps.Application.Locations.Abstractions;

namespace InfraOps.Application.Inventory.Commands.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandHandler : ICommandHandler<UpdateInventoryItemCommand, InventoryItemDetailsDto>
{
    private readonly IValidator<UpdateInventoryItemCommand> _validator;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly ILocationLookupRepository _locationLookupRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInventoryItemCommandHandler(
        IValidator<UpdateInventoryItemCommand> validator,
        IInventoryItemRepository inventoryItemRepository,
        IEntityTypeRepository entityTypeRepository,
        ILocationLookupRepository locationLookupRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _inventoryItemRepository = inventoryItemRepository;
        _entityTypeRepository = entityTypeRepository;
        _locationLookupRepository = locationLookupRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<InventoryItemDetailsDto> Handle(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.InventoryItemId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Inventory item was not found.");
        var entityType = await _entityTypeRepository.GetByIdAsync(inventoryItem.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");
        var region = await _locationLookupRepository.GetRegionByIdAsync(command.RegionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Region was not found.");
        var site = await _locationLookupRepository.GetSiteByIdAsync(command.SiteId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Site was not found.");

        InventoryStatusCatalog.TryParse(command.Status, out var status);

        inventoryItem.Update(
            entityType,
            region,
            site,
            command.DisplayName,
            status,
            command.InstallationDate,
            currentUserId,
            _clock.UtcNow,
            command.AttributeValues.Select(InventoryMappings.ToDraft).ToArray());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedInventoryItem = await _inventoryItemRepository.GetByIdAsync(command.InventoryItemId, cancellationToken)
            ?? inventoryItem;

        return InventoryMappings.ToDetailsDto(updatedInventoryItem);
    }
}
