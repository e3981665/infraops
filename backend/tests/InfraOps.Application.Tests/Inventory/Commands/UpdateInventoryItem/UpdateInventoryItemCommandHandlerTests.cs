using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Commands.UpdateInventoryItem;
using InfraOps.Application.Inventory.Support;
using InfraOps.Application.Locations.Abstractions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Application.Tests.Inventory.Commands.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandHandlerTests
{
    [Fact]
    public async Task Should_UpdateInventoryItemCorrectly_When_RequestIsValid()
    {
        var entityType = EntityType.Create(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups",
            null,
            [
                new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, []),
                new EntityFieldDefinitionDraft(null, "phaseType", "Phase Type", EntityFieldType.Select, 2, false, true, null, null, [new EntityFieldOptionDraft(null, "single-phase", "Single-phase", 1), new EntityFieldOptionDraft(null, "three-phase", "Three-phase", 2)])
            ]);
        var region = new Region(Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"), "north-region", "North Region");
        var site = new Site(Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"), region.Id, "north-hub", "North Hub");
        var inventoryItem = InventoryItem.Create(
            Guid.Parse("F916D166-C271-4D1C-8654-EA6736648D63"),
            entityType,
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            null,
            Guid.Parse("1B84A5A9-E7FB-4F66-965E-B75A1F0D18C4"),
            new DateTimeOffset(2026, 4, 2, 12, 0, 0, TimeSpan.Zero),
            [new InventoryAttributeValueDraft("serialNumber", "UPS-0001")]);

        var handler = new UpdateInventoryItemCommandHandler(
            new UpdateInventoryItemCommandValidator(),
            new StubInventoryItemRepository(inventoryItem),
            new StubEntityTypeRepository(entityType),
            new StubLocationLookupRepository(region, site),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new UpdateInventoryItemCommand(
                inventoryItem.Id,
                region.Id,
                site.Id,
                "UPS Room B",
                "maintenance",
                new DateOnly(2025, 5, 1),
                [
                    new InventoryAttributeValueInput("serialNumber", "UPS-0002"),
                    new InventoryAttributeValueInput("phaseType", "three-phase")
                ]),
            CancellationToken.None);

        Assert.Equal("UPS Room B", result.DisplayName);
        Assert.Equal("maintenance", result.Status);
        Assert.Equal("UPS-0002", inventoryItem.AttributeValues.Single(x => x.FieldKey == "serialNumber").Value);
        Assert.Equal("three-phase", inventoryItem.AttributeValues.Single(x => x.FieldKey == "phaseType").Value);
    }

    private sealed class StubInventoryItemRepository : IInventoryItemRepository
    {
        private readonly InventoryItem _inventoryItem;

        public StubInventoryItemRepository(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
        }

        public Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken)
            => Task.FromResult<InventoryItem?>(_inventoryItem);

        public Task<IReadOnlyCollection<InventoryItem>> ListAsync(InventoryListFilter filter, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<InventoryItem>>([]);
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
            => Task.FromResult<IReadOnlyCollection<EntityType>>([_entityType]);
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

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 4, 2, 13, 0, 0, TimeSpan.Zero);
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
