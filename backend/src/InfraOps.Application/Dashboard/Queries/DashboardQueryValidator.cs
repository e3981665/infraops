using FluentValidation;

namespace InfraOps.Application.Dashboard.Queries;

public abstract class DashboardQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IDashboardQuery
{
    protected DashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc <= x.ToUtc)
            .WithMessage("Dashboard date range is invalid.");
    }
}
