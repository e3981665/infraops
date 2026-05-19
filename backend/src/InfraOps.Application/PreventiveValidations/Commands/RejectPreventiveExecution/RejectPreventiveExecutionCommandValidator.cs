using FluentValidation;

namespace InfraOps.Application.PreventiveValidations.Commands.RejectPreventiveExecution;

public sealed class RejectPreventiveExecutionCommandValidator
    : AbstractValidator<RejectPreventiveExecutionCommand>
{
    public RejectPreventiveExecutionCommandValidator()
    {
        RuleFor(x => x.PreventiveExecutionId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
