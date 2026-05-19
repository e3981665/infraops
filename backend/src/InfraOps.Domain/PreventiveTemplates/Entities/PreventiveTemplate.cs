using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Domain.PreventiveTemplates.Entities;

public sealed class PreventiveTemplate
{
    private readonly List<PreventiveTemplateSection> _sections = [];

    private PreventiveTemplate()
    {
    }

    private PreventiveTemplate(
        Guid id,
        Guid entityTypeId,
        string name,
        string code,
        string? description)
    {
        Id = id;
        EntityTypeId = entityTypeId;
        Name = name;
        Code = code;
        Description = description;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public Guid EntityTypeId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public EntityType? EntityType { get; private set; }

    public IReadOnlyCollection<PreventiveTemplateSection> Sections => _sections;

    public static PreventiveTemplate Create(
        Guid id,
        EntityType entityType,
        string name,
        string code,
        string? description,
        IReadOnlyCollection<PreventiveTemplateSectionDraft> sections)
    {
        EnsureActiveEntityType(entityType);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Preventive template id is required.");
        }

        var template = new PreventiveTemplate(
            id,
            entityType.Id,
            NormalizeName(name),
            NormalizeCode(code),
            NormalizeDescription(description));

        template.SyncSections(sections);

        return template;
    }

    public void Update(
        EntityType entityType,
        string name,
        string code,
        string? description,
        IReadOnlyCollection<PreventiveTemplateSectionDraft> sections)
    {
        EnsureActiveEntityType(entityType);

        if (entityType.Id != EntityTypeId)
        {
            throw new DomainRuleException("Preventive template entity type cannot be changed after creation.");
        }

        Name = NormalizeName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);

        SyncSections(sections);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SyncSections(IReadOnlyCollection<PreventiveTemplateSectionDraft> sectionDrafts)
    {
        sectionDrafts ??= [];

        var sectionDisplayOrders = sectionDrafts.Select(x => x.DisplayOrder).ToArray();

        if (sectionDisplayOrders.Any(x => x <= 0))
        {
            throw new DomainRuleException("Preventive template section display order must be greater than zero.");
        }

        if (sectionDisplayOrders.Length != sectionDisplayOrders.Distinct().Count())
        {
            throw new DomainRuleException("Preventive template section display order must be unique.");
        }

        var normalizedItemKeys = sectionDrafts
            .SelectMany(x => x.ChecklistItems ?? [])
            .Select(x => PreventiveChecklistItem.NormalizeItemKey(x.ItemKey))
            .ToArray();

        if (normalizedItemKeys.Length != normalizedItemKeys.Distinct(StringComparer.Ordinal).Count())
        {
            throw new DomainRuleException("Preventive checklist item keys must be unique within a template.");
        }

        var existingSections = _sections.ToDictionary(x => x.Id, x => x);
        var processedIds = new HashSet<Guid>();

        foreach (var sectionDraft in sectionDrafts.OrderBy(x => x.DisplayOrder))
        {
            if (sectionDraft.Id is Guid sectionId && sectionId != Guid.Empty)
            {
                if (!existingSections.TryGetValue(sectionId, out var existingSection))
                {
                    throw new DomainRuleException("Preventive template section was not found for update.");
                }

                existingSection.Update(sectionDraft);
                processedIds.Add(existingSection.Id);
                continue;
            }

            var newSection = PreventiveTemplateSection.Create(Guid.NewGuid(), Id, sectionDraft);
            _sections.Add(newSection);
            processedIds.Add(newSection.Id);
        }

        foreach (var existingSection in _sections.Where(x => !processedIds.Contains(x.Id)))
        {
            existingSection.Deactivate();
        }
    }

    private static void EnsureActiveEntityType(EntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        if (!entityType.IsActive)
        {
            throw new DomainRuleException("Preventive templates must belong to an active entity type.");
        }
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("Preventive template name is required.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > 120)
        {
            throw new DomainRuleException("Preventive template name cannot exceed 120 characters.");
        }

        return normalizedName;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainRuleException("Preventive template code is required.");
        }

        var normalizedCode = code
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');

        while (normalizedCode.Contains("--", StringComparison.Ordinal))
        {
            normalizedCode = normalizedCode.Replace("--", "-", StringComparison.Ordinal);
        }

        normalizedCode = normalizedCode.Trim('-');

        if (normalizedCode.Length is 0 or > 60)
        {
            throw new DomainRuleException("Preventive template code must be between 1 and 60 characters.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedCode, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new DomainRuleException("Preventive template code must use lowercase letters, numbers, and hyphens only.");
        }

        return normalizedCode;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > 500)
        {
            throw new DomainRuleException("Preventive template description cannot exceed 500 characters.");
        }

        return normalizedDescription;
    }
}
