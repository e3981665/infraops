using FluentValidation;

namespace InfraOps.Application.PreventiveExecutions.Commands.StartPreventiveExecution;

public sealed class StartPreventiveExecutionCommandValidator : AbstractValidator<StartPreventiveExecutionCommand>
{
    public StartPreventiveExecutionCommandValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty();
    }
}
