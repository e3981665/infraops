using FluentValidation;

namespace InfraOps.Application.PreventiveExecutions.Support;

public sealed class PreventiveExecutionAnswerInputValidator : AbstractValidator<PreventiveExecutionAnswerInput>
{
    public PreventiveExecutionAnswerInputValidator()
    {
        RuleFor(x => x.ItemKey)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Value)
            .MaximumLength(2000);

        RuleFor(x => x.Comment)
            .MaximumLength(2000);
    }
}
