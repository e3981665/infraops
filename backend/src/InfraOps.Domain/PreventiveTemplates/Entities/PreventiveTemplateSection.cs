using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Domain.PreventiveTemplates.Entities;

public sealed class PreventiveTemplateSection
{
    private readonly List<PreventiveChecklistItem> _checklistItems = [];

    private PreventiveTemplateSection()
    {
    }

    private PreventiveTemplateSection(
        Guid id,
        Guid templateId,
        string title,
        int displayOrder,
        bool isActive)
    {
        Id = id;
        PreventiveTemplateId = templateId;
        Title = title;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }

    public Guid PreventiveTemplateId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<PreventiveChecklistItem> ChecklistItems => _checklistItems;

    public static PreventiveTemplateSection Create(
        Guid id,
        Guid templateId,
        PreventiveTemplateSectionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Preventive template section id is required.");
        }

        if (templateId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive template section template id is required.");
        }

        var section = new PreventiveTemplateSection(
            id,
            templateId,
            NormalizeTitle(draft.Title),
            NormalizeDisplayOrder(draft.DisplayOrder),
            draft.IsActive);

        section.SyncChecklistItems(draft.ChecklistItems);

        if (!section.IsActive)
        {
            foreach (var checklistItem in section._checklistItems)
            {
                checklistItem.Deactivate();
            }
        }

        return section;
    }

    public void Update(PreventiveTemplateSectionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        Title = NormalizeTitle(draft.Title);
        DisplayOrder = NormalizeDisplayOrder(draft.DisplayOrder);
        IsActive = draft.IsActive;

        SyncChecklistItems(draft.ChecklistItems);

        if (!IsActive)
        {
            foreach (var checklistItem in _checklistItems)
            {
                checklistItem.Deactivate();
            }
        }
    }

    public void Deactivate()
    {
        IsActive = false;

        foreach (var checklistItem in _checklistItems)
        {
            checklistItem.Deactivate();
        }
    }

    private void SyncChecklistItems(IReadOnlyCollection<PreventiveChecklistItemDraft> checklistItemDrafts)
    {
        checklistItemDrafts ??= [];

        var normalizedItemKeys = checklistItemDrafts
            .Select(x => PreventiveChecklistItem.NormalizeItemKey(x.ItemKey))
            .ToArray();

        if (normalizedItemKeys.Length != normalizedItemKeys.Distinct(StringComparer.Ordinal).Count())
        {
            throw new DomainRuleException("Preventive checklist item keys must be unique within a section.");
        }

        var displayOrders = checklistItemDrafts.Select(x => x.DisplayOrder).ToArray();

        if (displayOrders.Any(x => x <= 0))
        {
            throw new DomainRuleException("Preventive checklist item display order must be greater than zero.");
        }

        if (displayOrders.Length != displayOrders.Distinct().Count())
        {
            throw new DomainRuleException("Preventive checklist item display order must be unique within a section.");
        }

        var existingItems = _checklistItems.ToDictionary(x => x.Id, x => x);
        var processedIds = new HashSet<Guid>();

        foreach (var checklistItemDraft in checklistItemDrafts.OrderBy(x => x.DisplayOrder))
        {
            var normalizedItemKey = PreventiveChecklistItem.NormalizeItemKey(checklistItemDraft.ItemKey);

            var duplicateItem = _checklistItems.SingleOrDefault(x =>
                x.ItemKey == normalizedItemKey && x.Id != checklistItemDraft.Id.GetValueOrDefault());

            if (duplicateItem is not null)
            {
                throw new DomainRuleException("Preventive checklist item keys must be unique within a section.");
            }

            if (checklistItemDraft.Id is Guid checklistItemId && checklistItemId != Guid.Empty)
            {
                if (!existingItems.TryGetValue(checklistItemId, out var existingItem))
                {
                    throw new DomainRuleException("Preventive checklist item was not found for update.");
                }

                existingItem.Update(checklistItemDraft);
                processedIds.Add(existingItem.Id);
                continue;
            }

            var newItem = PreventiveChecklistItem.Create(Guid.NewGuid(), Id, checklistItemDraft);
            _checklistItems.Add(newItem);
            processedIds.Add(newItem.Id);
        }

        foreach (var existingItem in _checklistItems.Where(x => !processedIds.Contains(x.Id)))
        {
            existingItem.Deactivate();
        }
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleException("Preventive template section title is required.");
        }

        var normalizedTitle = title.Trim();

        if (normalizedTitle.Length > 120)
        {
            throw new DomainRuleException("Preventive template section title cannot exceed 120 characters.");
        }

        return normalizedTitle;
    }

    private static int NormalizeDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new DomainRuleException("Preventive template section display order must be greater than zero.");
        }

        return displayOrder;
    }
}
