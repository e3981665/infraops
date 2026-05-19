using FluentValidation;

namespace InfraOps.Application.PreventiveTemplates.Support;

public sealed class PreventiveTemplateSectionInputValidator : AbstractValidator<PreventiveTemplateSectionInput>
{
    public PreventiveTemplateSectionInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);

        RuleForEach(x => x.ChecklistItems)
            .SetValidator(new PreventiveChecklistItemInputValidator());
    }
}
