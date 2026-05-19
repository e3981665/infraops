using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Queries.ListInventoryItems;
using InfraOps.Application.Inventory.Support;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;

namespace InfraOps.Application.Tests.Inventory.Queries.ListInventoryItems;

public sealed class ListInventoryItemsQueryHandlerTests
{
    [Fact]
    public async Task Should_ListInventoryItemsWithFilters_When_RequestMatchesExistingItems()
    {
        var primaryEntityType = CreateEntityType(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups");
        var secondaryEntityType = CreateEntityType(
            Guid.Parse("C295E7FA-27A8-4D4D-B6B8-E6B8C8FFBE28"),
            "Generator",
            "generator");
        var region = new Region(
            Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"),
            "north-region",
            "North Region");
        var site = new Site(
            Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"),
            region.Id,
            "north-hub",
            "North Hub");

        var matchingItem = InventoryItem.Create(
            Guid.Parse("F916D166-C271-4D1C-8654-EA6736648D63"),
            primaryEntityType,
            region,
            site,
            "UPS Room A",
            InventoryStatus.Operational,
            new DateOnly(2024, 4, 1),
            Guid.Parse("1B84A5A9-E7FB-4F66-965E-B75A1F0D18C4"),
            new DateTimeOffset(2026, 4, 2, 12, 0, 0, TimeSpan.Zero),
            [new InventoryAttributeValueDraft("serialNumber", "UPS-0001")]);

        var nonMatchingItem = InventoryItem.Create(
            Guid.Parse("5923870A-C3D5-40D0-8EAA-EA43B12BC021"),
            secondaryEntityType,
            region,
            site,
            "Generator Yard",
            InventoryStatus.Maintenance,
            null,
            Guid.Parse("1B84A5A9-E7FB-4F66-965E-B75A1F0D18C4"),
            new DateTimeOffset(2026, 4, 2, 12, 0, 0, TimeSpan.Zero),
            [new InventoryAttributeValueDraft("serialNumber", "GEN-0001")]);

        var handler = new ListInventoryItemsQueryHandler(
            new ListInventoryItemsQueryValidator(),
            new StubInventoryItemRepository([matchingItem, nonMatchingItem]));

        var result = await handler.Handle(
            new ListInventoryItemsQuery(
                primaryEntityType.Id,
                "operational",
                site.Id,
                region.Id,
                true,
                "Room"),
            CancellationToken.None);

        var inventoryItem = Assert.Single(result);
        Assert.Equal(matchingItem.Id, inventoryItem.Id);
        Assert.Equal("UPS Room A", inventoryItem.DisplayName);
        Assert.Equal("operational", inventoryItem.Status);
    }

    private static EntityType CreateEntityType(Guid entityTypeId, string name, string code)
    {
        return EntityType.Create(
            entityTypeId,
            name,
            code,
            null,
            [new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, [])]);
    }

    private sealed class StubInventoryItemRepository : IInventoryItemRepository
    {
        private readonly IReadOnlyCollection<InventoryItem> _inventoryItems;

        public StubInventoryItemRepository(IReadOnlyCollection<InventoryItem> inventoryItems)
        {
            _inventoryItems = inventoryItems;
        }

        public Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("AddAsync is not used by list query tests.");
        }

        public Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_inventoryItems.SingleOrDefault(x => x.Id == inventoryItemId));
        }

        public Task<IReadOnlyCollection<InventoryItem>> ListAsync(InventoryListFilter filter, CancellationToken cancellationToken)
        {
            IEnumerable<InventoryItem> query = _inventoryItems;

            if (filter.EntityTypeId.HasValue)
            {
                query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.SiteId.HasValue)
            {
                query = query.Where(x => x.SiteId == filter.SiteId.Value);
            }

            if (filter.RegionId.HasValue)
            {
                query = query.Where(x => x.RegionId == filter.RegionId.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.DisplayName.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyCollection<InventoryItem>>(query.ToArray());
        }
    }
}
