using FluentValidation;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Commands.SubmitPreventiveExecution;

public sealed class SubmitPreventiveExecutionCommandValidator : AbstractValidator<SubmitPreventiveExecutionCommand>
{
    public SubmitPreventiveExecutionCommandValidator()
    {
        RuleFor(x => x.PreventiveExecutionId)
            .NotEmpty();

        RuleForEach(x => x.Answers)
            .SetValidator(new PreventiveExecutionAnswerInputValidator());
    }
}
