using FluentValidation;

namespace InfraOps.Application.PreventiveValidations.Commands.ApprovePreventiveExecution;

public sealed class ApprovePreventiveExecutionCommandValidator
    : AbstractValidator<ApprovePreventiveExecutionCommand>
{
    public ApprovePreventiveExecutionCommandValidator()
    {
        RuleFor(x => x.PreventiveExecutionId)
            .NotEmpty();

        RuleFor(x => x.Comment)
            .MaximumLength(2000);
    }
}
