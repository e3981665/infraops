using FluentValidation;

namespace InfraOps.Application.Inventory.Support;

public sealed class InventoryAttributeValueInputValidator : AbstractValidator<InventoryAttributeValueInput>
{
    public InventoryAttributeValueInputValidator()
    {
        RuleFor(x => x.FieldKey)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z][A-Za-z0-9]*$")
            .WithMessage("Field key must start with a lowercase letter and use only letters and numbers.");

        RuleFor(x => x.Value)
            .MaximumLength(5000);
    }
}
