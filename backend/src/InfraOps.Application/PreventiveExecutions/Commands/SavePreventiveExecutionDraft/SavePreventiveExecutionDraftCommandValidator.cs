using FluentValidation;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Commands.SavePreventiveExecutionDraft;

public sealed class SavePreventiveExecutionDraftCommandValidator : AbstractValidator<SavePreventiveExecutionDraftCommand>
{
    public SavePreventiveExecutionDraftCommandValidator()
    {
        RuleFor(x => x.PreventiveExecutionId)
            .NotEmpty();

        RuleForEach(x => x.Answers)
            .SetValidator(new PreventiveExecutionAnswerInputValidator());
    }
}
