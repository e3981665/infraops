using FluentValidation;

namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed class PreventiveChecklistItemInputValidator : AbstractValidator<PreventiveChecklistItemInput>
{
    public PreventiveChecklistItemInputValidator()
    {
        RuleFor(x => x.ItemKey)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z][A-Za-z0-9]*$")
            .WithMessage("Checklist item key must start with a lowercase letter and use only letters and numbers.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.ItemType)
            .Must(value => PreventiveChecklistItemTypeCatalog.TryParse(value, out _))
            .WithMessage($"Checklist item type must be one of: {string.Join(", ", PreventiveChecklistItemTypeCatalog.SupportedValues)}.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);

        RuleFor(x => x.HelpText)
            .MaximumLength(500);

        RuleForEach(x => x.Options)
            .SetValidator(new PreventiveChecklistOptionInputValidator());

        RuleFor(x => x)
            .Must(x => HasValidOptions(x))
            .WithMessage("Select checklist items require options, and non-select items cannot define options.");

        RuleFor(x => x)
            .Must(x => HasValidNumericBounds(x))
            .WithMessage("Only numeric checklist items can define minimum or maximum values, and minimum cannot be greater than maximum.");
    }

    private static bool HasValidOptions(PreventiveChecklistItemInput input)
    {
        var isSelect = string.Equals(input.ItemType, "select", StringComparison.OrdinalIgnoreCase);
        var options = input.Options ?? [];

        return isSelect ? options.Count > 0 : options.Count == 0;
    }

    private static bool HasValidNumericBounds(PreventiveChecklistItemInput input)
    {
        var isNumeric = string.Equals(input.ItemType, "numeric", StringComparison.OrdinalIgnoreCase);

        if (isNumeric)
        {
            return !input.MinimumValue.HasValue
                || !input.MaximumValue.HasValue
                || input.MinimumValue.Value <= input.MaximumValue.Value;
        }

        return !input.MinimumValue.HasValue && !input.MaximumValue.HasValue;
    }
}
