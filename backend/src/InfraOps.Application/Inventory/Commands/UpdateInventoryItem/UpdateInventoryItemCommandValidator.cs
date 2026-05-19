using FluentValidation;
using InfraOps.Application.Inventory.Support;

namespace InfraOps.Application.Inventory.Commands.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandValidator : AbstractValidator<UpdateInventoryItemCommand>
{
    public UpdateInventoryItemCommandValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty();

        RuleFor(x => x.RegionId)
            .NotEmpty();

        RuleFor(x => x.SiteId)
            .NotEmpty();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Status)
            .Must(value => InventoryStatusCatalog.TryParse(value, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", InventoryStatusCatalog.SupportedValues)}.");

        RuleForEach(x => x.AttributeValues)
            .SetValidator(new InventoryAttributeValueInputValidator());
    }
}
