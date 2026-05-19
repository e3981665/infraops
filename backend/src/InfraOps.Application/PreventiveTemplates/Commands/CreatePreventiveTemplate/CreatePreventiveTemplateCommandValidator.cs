using FluentValidation;
using InfraOps.Application.PreventiveTemplates.Support;

namespace InfraOps.Application.PreventiveTemplates.Commands.CreatePreventiveTemplate;

public sealed class CreatePreventiveTemplateCommandValidator : AbstractValidator<CreatePreventiveTemplateCommand>
{
    public CreatePreventiveTemplateCommandValidator()
    {
        RuleFor(x => x.EntityTypeId)
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
