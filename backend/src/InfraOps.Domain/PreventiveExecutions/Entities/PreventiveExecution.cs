using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.PreventiveExecutions.Enums;
using InfraOps.Domain.PreventiveExecutions.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveExecution
{
    private readonly List<PreventiveExecutionTemplateSection> _templateSections = [];
    private readonly List<PreventiveExecutionAnswer> _answers = [];
    private readonly List<PreventiveValidationRecord> _validationRecords = [];

    private PreventiveExecution()
    {
    }

    private PreventiveExecution(
        Guid id,
        InventoryItem inventoryItem,
        PreventiveTemplate preventiveTemplate,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        InventoryItemId = inventoryItem.Id;
        PreventiveTemplateId = preventiveTemplate.Id;
        PreventiveTemplateName = preventiveTemplate.Name;
        PreventiveTemplateCode = preventiveTemplate.Code;
        EntityTypeId = inventoryItem.EntityTypeId;
        EntityTypeName = inventoryItem.EntityType?.Name ?? string.Empty;
        EntityTypeCode = inventoryItem.EntityType?.Code ?? string.Empty;
        InventoryItemDisplayName = inventoryItem.DisplayName;
        RegionId = inventoryItem.RegionId;
        RegionName = inventoryItem.Region?.Name ?? string.Empty;
        SiteId = inventoryItem.SiteId;
        SiteName = inventoryItem.Site?.Name ?? string.Empty;
        Status = PreventiveExecutionStatus.Draft;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid InventoryItemId { get; private set; }

    public Guid PreventiveTemplateId { get; private set; }

    public string PreventiveTemplateName { get; private set; } = string.Empty;

    public string PreventiveTemplateCode { get; private set; } = string.Empty;

    public Guid EntityTypeId { get; private set; }

    public string EntityTypeName { get; private set; } = string.Empty;

    public string EntityTypeCode { get; private set; } = string.Empty;

    public string InventoryItemDisplayName { get; private set; } = string.Empty;

    public Guid RegionId { get; private set; }

    public string RegionName { get; private set; } = string.Empty;

    public Guid SiteId { get; private set; }

    public string SiteName { get; private set; } = string.Empty;

    public PreventiveExecutionStatus Status { get; private set; }

    public Guid CreatedBy { get; private set; }

    public Guid UpdatedBy { get; private set; }

    public Guid? SubmittedBy { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public InventoryItem? InventoryItem { get; private set; }

    public PreventiveTemplate? PreventiveTemplate { get; private set; }

    public IReadOnlyCollection<PreventiveExecutionTemplateSection> TemplateSections => _templateSections;

    public IReadOnlyCollection<PreventiveExecutionAnswer> Answers => _answers;

    public IReadOnlyCollection<PreventiveValidationRecord> ValidationRecords => _validationRecords;

    public static PreventiveExecution CreateDraft(
        Guid id,
        InventoryItem inventoryItem,
        PreventiveTemplate preventiveTemplate,
        Guid createdBy,
        DateTimeOffset createdAtUtc)
    {
        EnsureValidStart(id, inventoryItem, preventiveTemplate, createdBy);

        var execution = new PreventiveExecution(id, inventoryItem, preventiveTemplate, createdBy, createdAtUtc);

        foreach (var section in preventiveTemplate.Sections.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder))
        {
            var snapshotSection = PreventiveExecutionTemplateSection.Create(execution.Id, section);

            if (snapshotSection.ChecklistItems.Count > 0)
            {
                execution._templateSections.Add(snapshotSection);
            }
        }

        if (!execution.GetTemplateItems().Any())
        {
            throw new DomainRuleException("Preventive execution requires an active template with active checklist items.");
        }

        return execution;
    }

    public void UpdateDraft(
        IReadOnlyCollection<PreventiveExecutionAnswerDraft> answers,
        Guid updatedBy,
        DateTimeOffset updatedAtUtc)
    {
        EnsureDraftEditable();
        EnsureUser(updatedBy, "Preventive execution updater is required.");
        SyncAnswers(answers);

        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Submit(
        IReadOnlyCollection<PreventiveExecutionAnswerDraft>? answers,
        Guid submittedBy,
        DateTimeOffset submittedAtUtc)
    {
        EnsureDraftEditable();
        EnsureUser(submittedBy, "Preventive execution submitter is required.");

        if (answers is not null)
        {
            SyncAnswers(answers);
        }

        EnsureSubmissionComplete();

        Status = PreventiveExecutionStatus.Submitted;
        SubmittedBy = submittedBy;
        UpdatedBy = submittedBy;
        SubmittedAtUtc = submittedAtUtc;
        UpdatedAtUtc = submittedAtUtc;
    }

    public void Approve(Guid validatorUserId, DateTimeOffset validatedAtUtc, string? comment)
    {
        EnsureValidatable();
        EnsureUser(validatorUserId, "Preventive validation user is required.");

        Status = PreventiveExecutionStatus.Approved;
        UpdatedBy = validatorUserId;
        UpdatedAtUtc = validatedAtUtc;
        _validationRecords.Add(PreventiveValidationRecord.Create(
            Id,
            PreventiveValidationActionType.Approved,
            validatorUserId,
            validatedAtUtc,
            comment));
    }

    public void Reject(Guid validatorUserId, DateTimeOffset validatedAtUtc, string reason)
    {
        EnsureValidatable();
        EnsureUser(validatorUserId, "Preventive validation user is required.");
        EnsureRequiredReason(reason, "Preventive rejection reason is required.");

        Status = PreventiveExecutionStatus.Rejected;
        UpdatedBy = validatorUserId;
        UpdatedAtUtc = validatedAtUtc;
        _validationRecords.Add(PreventiveValidationRecord.Create(
            Id,
            PreventiveValidationActionType.Rejected,
            validatorUserId,
            validatedAtUtc,
            reason));
    }

    public void RequestRework(Guid validatorUserId, DateTimeOffset validatedAtUtc, string reason)
    {
        EnsureValidatable();
        EnsureUser(validatorUserId, "Preventive validation user is required.");
        EnsureRequiredReason(reason, "Preventive rework reason is required.");

        Status = PreventiveExecutionStatus.ReworkRequested;
        UpdatedBy = validatorUserId;
        UpdatedAtUtc = validatedAtUtc;
        _validationRecords.Add(PreventiveValidationRecord.Create(
            Id,
            PreventiveValidationActionType.ReworkRequested,
            validatorUserId,
            validatedAtUtc,
            reason));
    }

    private void SyncAnswers(IReadOnlyCollection<PreventiveExecutionAnswerDraft>? answerDrafts)
    {
        answerDrafts ??= [];

        var templateItemsByKey = GetTemplateItems().ToDictionary(
            x => x.ItemKey,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        var duplicateAnswer = answerDrafts
            .GroupBy(x => x.ItemKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateAnswer is not null)
        {
            throw new DomainRuleException("Preventive execution answers must use unique checklist item keys.");
        }

        foreach (var answerDraft in answerDrafts)
        {
            if (!templateItemsByKey.ContainsKey(answerDraft.ItemKey))
            {
                throw new DomainRuleException($"Checklist item '{answerDraft.ItemKey}' is not defined by the execution template snapshot.");
            }
        }

        var existingAnswersByKey = _answers.ToDictionary(x => x.ItemKey, x => x, StringComparer.OrdinalIgnoreCase);
        var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var answerDraft in answerDrafts)
        {
            var templateItem = templateItemsByKey[answerDraft.ItemKey];

            if (existingAnswersByKey.TryGetValue(templateItem.ItemKey, out var existingAnswer))
            {
                existingAnswer.Update(templateItem, answerDraft.Value, answerDraft.Comment);
            }
            else
            {
                _answers.Add(PreventiveExecutionAnswer.Create(
                    Id,
                    templateItem,
                    answerDraft.Value,
                    answerDraft.Comment));
            }

            processedKeys.Add(templateItem.ItemKey);
        }

        _answers.RemoveAll(x => !processedKeys.Contains(x.ItemKey));
    }

    private void EnsureSubmissionComplete()
    {
        var answersByKey = _answers.ToDictionary(x => x.ItemKey, x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var requiredItem in GetTemplateItems().Where(x => x.IsRequired))
        {
            if (!answersByKey.TryGetValue(requiredItem.ItemKey, out var answer)
                || !PreventiveExecutionAnswer.HasProvidedValue(answer.Value))
            {
                throw new DomainRuleException($"Checklist item '{requiredItem.ItemKey}' is required before submission.");
            }
        }
    }

    private IEnumerable<PreventiveExecutionTemplateItem> GetTemplateItems()
    {
        return _templateSections.SelectMany(x => x.ChecklistItems);
    }

    private void EnsureDraftEditable()
    {
        if (Status != PreventiveExecutionStatus.Draft)
        {
            throw new DomainRuleException("Only draft preventive executions can be modified.");
        }
    }

    private void EnsureValidatable()
    {
        if (Status != PreventiveExecutionStatus.Submitted)
        {
            throw new DomainRuleException("Only submitted preventive executions can be validated.");
        }
    }

    private static void EnsureValidStart(
        Guid id,
        InventoryItem inventoryItem,
        PreventiveTemplate preventiveTemplate,
        Guid createdBy)
    {
        ArgumentNullException.ThrowIfNull(inventoryItem);
        ArgumentNullException.ThrowIfNull(preventiveTemplate);

        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution id is required.");
        }

        EnsureUser(createdBy, "Preventive execution creator is required.");

        if (!inventoryItem.IsActive)
        {
            throw new DomainRuleException("Preventive execution must belong to an active inventory item.");
        }

        if (!preventiveTemplate.IsActive)
        {
            throw new DomainRuleException("Preventive execution requires an active preventive template.");
        }

        if (preventiveTemplate.EntityTypeId != inventoryItem.EntityTypeId)
        {
            throw new DomainRuleException("Preventive template entity type must match the inventory item entity type.");
        }
    }

    private static void EnsureUser(Guid userId, string message)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainRuleException(message);
        }
    }

    private static void EnsureRequiredReason(string? reason, string message)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException(message);
        }
    }
}
