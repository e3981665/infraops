using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Dtos;
using InfraOps.Application.Inventory.Support;
using FluentValidation;
using InfraOps.Domain.Inventory.Enums;

namespace InfraOps.Application.Inventory.Queries.ListInventoryItems;

public sealed class ListInventoryItemsQueryHandler : IQueryHandler<ListInventoryItemsQuery, IReadOnlyCollection<InventoryItemSummaryDto>>
{
    private readonly IValidator<ListInventoryItemsQuery> _validator;
    private readonly IInventoryItemRepository _inventoryItemRepository;

    public ListInventoryItemsQueryHandler(
        IValidator<ListInventoryItemsQuery> validator,
        IInventoryItemRepository inventoryItemRepository)
    {
        _validator = validator;
        _inventoryItemRepository = inventoryItemRepository;
    }

    public async Task<IReadOnlyCollection<InventoryItemSummaryDto>> Handle(
        ListInventoryItemsQuery query,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        InventoryStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            InventoryStatusCatalog.TryParse(query.Status, out var status);
            parsedStatus = status;
        }

        var inventoryItems = await _inventoryItemRepository.ListAsync(
            new InventoryListFilter(
                query.EntityTypeId,
                parsedStatus,
                query.SiteId,
                query.RegionId,
                query.IsActive,
                query.Search),
            cancellationToken);

        return inventoryItems
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(InventoryMappings.ToSummaryDto)
            .ToArray();
    }
}
