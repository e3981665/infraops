using FluentValidation;

namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed class PreventiveChecklistOptionInputValidator : AbstractValidator<PreventiveChecklistOptionInput>
{
    public PreventiveChecklistOptionInputValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-zA-Z0-9\\s_-]+$")
            .WithMessage("Option value can only use letters, numbers, spaces, underscores, and hyphens before normalization.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);
    }
}
