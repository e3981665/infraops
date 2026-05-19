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
using InfraOps.Domain.Inventory.Entities;

namespace InfraOps.Application.Inventory.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandHandler : ICommandHandler<CreateInventoryItemCommand, InventoryItemDetailsDto>
{
    private readonly IValidator<CreateInventoryItemCommand> _validator;
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly ILocationLookupRepository _locationLookupRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryItemCommandHandler(
        IValidator<CreateInventoryItemCommand> validator,
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
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var entityType = await _entityTypeRepository.GetByIdAsync(command.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");
        var region = await _locationLookupRepository.GetRegionByIdAsync(command.RegionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Region was not found.");
        var site = await _locationLookupRepository.GetSiteByIdAsync(command.SiteId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Site was not found.");

        InventoryStatusCatalog.TryParse(command.Status, out var status);

        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            entityType,
            region,
            site,
            command.DisplayName,
            status,
            command.InstallationDate,
            currentUserId,
            _clock.UtcNow,
            command.AttributeValues.Select(InventoryMappings.ToDraft).ToArray());

        await _inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdInventoryItem = await _inventoryItemRepository.GetByIdAsync(inventoryItem.Id, cancellationToken)
            ?? inventoryItem;

        return InventoryMappings.ToDetailsDto(createdInventoryItem);
    }
}
