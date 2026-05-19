using FluentValidation;

namespace InfraOps.Application.EntityTypes.Support;

public sealed class EntityFieldOptionInputValidator : AbstractValidator<EntityFieldOptionInput>
{
    public EntityFieldOptionInputValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);
    }
}
