using FluentValidation;

namespace InfraOps.Application.PreventiveValidations.Commands.RequestPreventiveRework;

public sealed class RequestPreventiveReworkCommandValidator
    : AbstractValidator<RequestPreventiveReworkCommand>
{
    public RequestPreventiveReworkCommandValidator()
    {
        RuleFor(x => x.PreventiveExecutionId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
