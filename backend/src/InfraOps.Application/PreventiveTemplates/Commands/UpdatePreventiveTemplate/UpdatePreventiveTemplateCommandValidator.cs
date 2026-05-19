using FluentValidation;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Commands.UpdatePreventiveTemplate;

public sealed class UpdatePreventiveTemplateCommandValidator : AbstractValidator<UpdatePreventiveTemplateCommand>
{
    public UpdatePreventiveTemplateCommandValidator()
    {
        RuleFor(x => x.PreventiveTemplateId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(60)
            .Matches("^[a-zA-Z0-9\\s_-]+$")
            .WithMessage("Preventive template code can only use letters, numbers, spaces, underscores, and hyphens before normalization.");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleForEach(x => x.Sections)
            .SetValidator(new PreventiveTemplateSectionInputValidator());
    }
}
