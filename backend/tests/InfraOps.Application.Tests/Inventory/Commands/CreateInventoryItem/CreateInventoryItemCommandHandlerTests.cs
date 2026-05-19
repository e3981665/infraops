using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Commands.CreateInventoryItem;
using InfraOps.Application.Inventory.Support;
using InfraOps.Application.Locations.Abstractions;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Application.Tests.Inventory.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandHandlerTests
{
    [Fact]
    public async Task Should_PersistValidInventoryItem_When_RequestIsValid()
    {
        var entityType = CreateEntityType();
        var region = CreateRegion();
        var site = CreateSite(region.Id);
        var inventoryRepository = new StubInventoryItemRepository();
        var handler = new CreateInventoryItemCommandHandler(
            new CreateInventoryItemCommandValidator(),
            inventoryRepository,
            new StubEntityTypeRepository(entityType),
            new StubLocationLookupRepository(region, site),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new CreateInventoryItemCommand(
                entityType.Id,
                region.Id,
                site.Id,
                "UPS Room A",
                "operational",
                new DateOnly(2024, 4, 1),
                [new InventoryAttributeValueInput("serialNumber", "UPS-0001")]),
            CancellationToken.None);

        Assert.Equal("UPS Room A", result.DisplayName);
        Assert.NotNull(inventoryRepository.AddedInventoryItem);
        Assert.Single(inventoryRepository.AddedInventoryItem!.AttributeValues);
    }

    [Fact]
    public async Task Should_RejectCreation_When_EntityTypeIsInactive()
    {
        var inactiveEntityType = CreateEntityType();
        inactiveEntityType.Deactivate();
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var handler = new CreateInventoryItemCommandHandler(
            new CreateInventoryItemCommandValidator(),
            new StubInventoryItemRepository(),
            new StubEntityTypeRepository(inactiveEntityType),
            new StubLocationLookupRepository(region, site),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => handler.Handle(
            new CreateInventoryItemCommand(
                inactiveEntityType.Id,
                region.Id,
                site.Id,
                "UPS Room A",
                "operational",
                null,
                [new InventoryAttributeValueInput("serialNumber", "UPS-0001")]),
            CancellationToken.None));

        Assert.Equal("Inventory items must belong to an active entity type.", exception.Message);
    }

    [Fact]
    public async Task Should_RejectCreation_When_CurrentUserIsMissing()
    {
        var entityType = CreateEntityType();
        var region = CreateRegion();
        var site = CreateSite(region.Id);

        var handler = new CreateInventoryItemCommandHandler(
            new CreateInventoryItemCommandValidator(),
            new StubInventoryItemRepository(),
            new StubEntityTypeRepository(entityType),
            new StubLocationLookupRepository(region, site),
            new AnonymousCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        await Assert.ThrowsAsync<ApplicationUnauthorizedException>(() => handler.Handle(
            new CreateInventoryItemCommand(
                entityType.Id,
                region.Id,
                site.Id,
                "UPS Room A",
                "operational",
                null,
                [new InventoryAttributeValueInput("serialNumber", "UPS-0001")]),
            CancellationToken.None));
    }

    private sealed class StubInventoryItemRepository : IInventoryItemRepository
    {
        public InventoryItem? AddedInventoryItem { get; private set; }

        public Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
        {
            AddedInventoryItem = inventoryItem;
            return Task.CompletedTask;
        }

        public Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(AddedInventoryItem);
        }

        public Task<IReadOnlyCollection<InventoryItem>> ListAsync(InventoryListFilter filter, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<InventoryItem>>([]);
        }
    }

    private sealed class StubEntityTypeRepository : IEntityTypeRepository
    {
        private readonly EntityType _entityType;

        public StubEntityTypeRepository(EntityType entityType)
        {
            _entityType = entityType;
        }

        public Task AddAsync(EntityType entityType, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken)
            => Task.FromResult<EntityType?>(_entityType);

        public Task<bool> IsCodeInUseAsync(string normalizedCode, Guid? excludedEntityTypeId, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<EntityType>>([]);

        public Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<EntityType>>(_entityType.IsActive ? [_entityType] : []);
    }

    private sealed class StubLocationLookupRepository : ILocationLookupRepository
    {
        private readonly Region _region;
        private readonly Site _site;

        public StubLocationLookupRepository(Region region, Site site)
        {
            _region = region;
            _site = site;
        }

        public Task<Region?> GetRegionByIdAsync(Guid regionId, CancellationToken cancellationToken)
            => Task.FromResult<Region?>(_region);

        public Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken)
            => Task.FromResult<Site?>(_site);

        public Task<IReadOnlyCollection<Region>> ListActiveRegionsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<Region>>([_region]);

        public Task<IReadOnlyCollection<Site>> ListActiveSitesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<Site>>([_site]);
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => true;

        public Guid? UserId => Guid.Parse("1B84A5A9-E7FB-4F66-965E-B75A1F0D18C4");
    }

    private sealed class AnonymousCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => false;

        public Guid? UserId => null;
    }

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 4, 2, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups",
            null,
            [new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, [])]);
    }

    private static Region CreateRegion()
    {
        return new Region(Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"), "north-region", "North Region");
    }

    private static Site CreateSite(Guid regionId)
    {
        return new Site(Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"), regionId, "north-hub", "North Hub");
    }
}
