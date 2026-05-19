using FluentValidation;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveValidations.Queries.ListPreventiveValidations;

public sealed class ListPreventiveValidationsQueryValidator
    : AbstractValidator<ListPreventiveValidationsQuery>
{
    public ListPreventiveValidationsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || PreventiveExecutionStatusCatalog.TryParse(value, out _))
            .WithMessage($"Execution status must be one of: {string.Join(", ", PreventiveExecutionStatusCatalog.SupportedValues)}.");

        RuleFor(x => x.Search)
            .MaximumLength(120);

        RuleFor(x => x)
            .Must(x => !x.SubmittedFromUtc.HasValue || !x.SubmittedToUtc.HasValue || x.SubmittedFromUtc <= x.SubmittedToUtc)
            .WithMessage("Submitted date range is invalid.");
    }
}
