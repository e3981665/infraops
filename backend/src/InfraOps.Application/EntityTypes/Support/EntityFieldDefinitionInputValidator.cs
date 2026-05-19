using FluentValidation;

namespace InfraOps.Application.EntityTypes.Support;

public sealed class EntityFieldDefinitionInputValidator : AbstractValidator<EntityFieldDefinitionInput>
{
    public EntityFieldDefinitionInputValidator()
    {
        RuleFor(x => x.FieldKey)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z][A-Za-z0-9]*$")
            .WithMessage("Field key must start with a lowercase letter and use only letters and numbers.");

        RuleFor(x => x.DisplayLabel)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.FieldType)
            .Must(value => EntityFieldTypeCatalog.TryParse(value, out _))
            .WithMessage($"Field type must be one of: {string.Join(", ", EntityFieldTypeCatalog.SupportedValues)}.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);

        RuleFor(x => x.Placeholder)
            .MaximumLength(200);

        RuleFor(x => x.HelpText)
            .MaximumLength(500);

        RuleForEach(x => x.Options)
            .SetValidator(new EntityFieldOptionInputValidator());

        RuleFor(x => x.Options)
            .Must((definition, options) => definition.FieldType.Equals("select", StringComparison.OrdinalIgnoreCase)
                ? options.Count > 0
                : options.Count == 0)
            .WithMessage("Select fields require at least one option, and non-select fields cannot define options.");
    }
}
