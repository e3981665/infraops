using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveExecutionTemplateSection
{
    private readonly List<PreventiveExecutionTemplateItem> _checklistItems = [];

    private PreventiveExecutionTemplateSection()
    {
    }

    private PreventiveExecutionTemplateSection(
        Guid id,
        Guid executionId,
        Guid sourceTemplateSectionId,
        string title,
        int displayOrder)
    {
        Id = id;
        PreventiveExecutionId = executionId;
        SourceTemplateSectionId = sourceTemplateSectionId;
        Title = title;
        DisplayOrder = displayOrder;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveExecutionId { get; private set; }

    public Guid SourceTemplateSectionId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public IReadOnlyCollection<PreventiveExecutionTemplateItem> ChecklistItems => _checklistItems;

    public static PreventiveExecutionTemplateSection Create(
        Guid executionId,
        PreventiveTemplateSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (executionId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution template section execution id is required.");
        }

        var snapshotSection = new PreventiveExecutionTemplateSection(
            Guid.NewGuid(),
            executionId,
            section.Id,
            section.Title,
            section.DisplayOrder);

        foreach (var item in section.ChecklistItems.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder))
        {
            snapshotSection._checklistItems.Add(PreventiveExecutionTemplateItem.Create(snapshotSection.Id, item));
        }

        return snapshotSection;
    }
}
