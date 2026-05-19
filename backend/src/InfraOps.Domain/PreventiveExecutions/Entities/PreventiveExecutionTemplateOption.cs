using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveExecutionTemplateOption
{
    private PreventiveExecutionTemplateOption()
    {
    }

    private PreventiveExecutionTemplateOption(
        Guid id,
        Guid templateItemId,
        string value,
        string label,
        int displayOrder)
    {
        Id = id;
        PreventiveExecutionTemplateItemId = templateItemId;
        Value = value;
        Label = label;
        DisplayOrder = displayOrder;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveExecutionTemplateItemId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string Label { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public static PreventiveExecutionTemplateOption Create(
        Guid templateItemId,
        PreventiveChecklistOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        if (templateItemId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution template option item id is required.");
        }

        return new PreventiveExecutionTemplateOption(
            Guid.NewGuid(),
            templateItemId,
            option.Value,
            option.Label,
            option.DisplayOrder);
    }
}
