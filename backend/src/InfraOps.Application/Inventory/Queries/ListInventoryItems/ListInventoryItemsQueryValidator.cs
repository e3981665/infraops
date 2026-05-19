using FluentValidation;

namespace InfraOps.Application.Inventory.Queries.ListInventoryItems;

public sealed class ListInventoryItemsQueryValidator : AbstractValidator<ListInventoryItemsQuery>
{
    public ListInventoryItemsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || Support.InventoryStatusCatalog.TryParse(value, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", Support.InventoryStatusCatalog.SupportedValues)}.");

        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}
