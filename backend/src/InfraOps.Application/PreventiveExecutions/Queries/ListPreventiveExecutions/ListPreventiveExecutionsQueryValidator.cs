using FluentValidation;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Queries.ListPreventiveExecutions;

public sealed class ListPreventiveExecutionsQueryValidator : AbstractValidator<ListPreventiveExecutionsQuery>
{
    public ListPreventiveExecutionsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || PreventiveExecutionStatusCatalog.TryParse(value, out _))
            .WithMessage($"Execution status must be one of: {string.Join(", ", PreventiveExecutionStatusCatalog.SupportedValues)}.");

        RuleFor(x => x.Search)
            .MaximumLength(120);

        RuleFor(x => x)
            .Must(x => !x.StartedFromUtc.HasValue || !x.StartedToUtc.HasValue || x.StartedFromUtc <= x.StartedToUtc)
            .WithMessage("Start date range is invalid.");

        RuleFor(x => x)
            .Must(x => !x.SubmittedFromUtc.HasValue || !x.SubmittedToUtc.HasValue || x.SubmittedFromUtc <= x.SubmittedToUtc)
            .WithMessage("Submitted date range is invalid.");
    }
}
