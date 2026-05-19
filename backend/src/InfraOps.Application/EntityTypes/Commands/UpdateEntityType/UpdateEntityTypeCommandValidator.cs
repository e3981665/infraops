using FluentValidation;
using InfraOps.Application.EntityTypes.Support;

namespace InfraOps.Application.EntityTypes.Commands.UpdateEntityType;

public sealed class UpdateEntityTypeCommandValidator : AbstractValidator<UpdateEntityTypeCommand>
{
    public UpdateEntityTypeCommandValidator()
    {
        RuleFor(x => x.EntityTypeId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(60)
            .Matches("^[a-zA-Z0-9\\s_-]+$")
            .WithMessage("Entity type code can only use letters, numbers, spaces, underscores, and hyphens before normalization.");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleForEach(x => x.FieldDefinitions)
            .SetValidator(new EntityFieldDefinitionInputValidator());
    }
}
