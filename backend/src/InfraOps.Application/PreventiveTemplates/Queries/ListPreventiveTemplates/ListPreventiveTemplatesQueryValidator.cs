using FluentValidation;

namespace InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplates;

public sealed class ListPreventiveTemplatesQueryValidator : AbstractValidator<ListPreventiveTemplatesQuery>
{
    public ListPreventiveTemplatesQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200);
    }
}
