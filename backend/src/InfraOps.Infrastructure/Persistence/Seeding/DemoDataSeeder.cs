using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Identity.Entities;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveExecutions.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;
using InfraOps.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InfraOps.Infrastructure.Persistence.Seeding;

public sealed class DemoDataSeeder
{
    private readonly InfraOpsDbContext _dbContext;
    private readonly DemoSeedLabels _labels;

    public DemoDataSeeder(
        InfraOpsDbContext dbContext,
        IOptions<DevelopmentSeedOptions> developmentSeedOptions)
    {
        _dbContext = dbContext;
        _labels = DemoSeedLabels.ForLocale(developmentSeedOptions.Value.DemoContentLocale);
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.EntityTypes.AnyAsync(x => x.Code == "ups", cancellationToken)
            && await _dbContext.PreventiveExecutions.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = await GetUserAsync("admin@infraops.local", cancellationToken);
        var technician = await GetUserAsync("technician@infraops.local", cancellationToken);
        var validator = await GetUserAsync("validator@infraops.local", cancellationToken);
        var regions = await _dbContext.Regions.ToDictionaryAsync(x => x.Code, cancellationToken);
        var sites = await _dbContext.Sites.ToDictionaryAsync(x => x.Code, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var ups = await EnsureEntityTypeAsync(_labels.UpsName, "ups", _labels.UpsDescription, admin.Id, cancellationToken);
        var generator = await EnsureEntityTypeAsync(_labels.GeneratorName, "generator", _labels.GeneratorDescription, admin.Id, cancellationToken);
        var hvac = await EnsureEntityTypeAsync(_labels.HvacName, "hvac", _labels.HvacDescription, admin.Id, cancellationToken);

        await EnsureTemplateAsync(ups, _labels.UpsTemplateName, "quarterly-ups-inspection", _labels.UpsTemplateDescription, cancellationToken);
        await EnsureTemplateAsync(generator, _labels.GeneratorTemplateName, "monthly-generator-run", _labels.GeneratorTemplateDescription, cancellationToken);
        await EnsureTemplateAsync(hvac, _labels.HvacTemplateName, "monthly-hvac-inspection", _labels.HvacTemplateDescription, cancellationToken);

        await EnsureInventoryItemAsync(ups, regions["north-region"], sites["north-hub"], "UPS-01", technician.Id, now.AddMonths(-8), cancellationToken);
        await EnsureInventoryItemAsync(ups, regions["north-region"], sites["riverside-station"], "UPS-02", technician.Id, now.AddMonths(-7), cancellationToken);
        await EnsureInventoryItemAsync(generator, regions["south-region"], sites["south-yard"], "GEN-01", technician.Id, now.AddMonths(-6), cancellationToken);
        await EnsureInventoryItemAsync(generator, regions["north-region"], sites["north-hub"], "GEN-02", technician.Id, now.AddMonths(-4), cancellationToken);
        await EnsureInventoryItemAsync(hvac, regions["south-region"], sites["south-yard"], "HVAC-01", technician.Id, now.AddMonths(-5), cancellationToken);
        await EnsureInventoryItemAsync(hvac, regions["north-region"], sites["riverside-station"], "HVAC-02", technician.Id, now.AddMonths(-3), cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await EnsureDemoExecutionsAsync(technician.Id, validator.Id, now, cancellationToken);
    }

    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(email);

        return await _dbContext.Users.SingleAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    private async Task<EntityType> EnsureEntityTypeAsync(
        string name,
        string code,
        string description,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        var entityType = await _dbContext.EntityTypes
            .Include(x => x.FieldDefinitions)
            .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Code == code, cancellationToken);

        var fields = new[]
        {
            new EntityFieldDefinitionDraft(null, "serialNumber", _labels.SerialNumberLabel, EntityFieldType.Text, 1, true, true, null, _labels.SerialNumberHelpText, []),
            new EntityFieldDefinitionDraft(null, "manufacturer", _labels.ManufacturerLabel, EntityFieldType.Text, 2, false, true, null, null, []),
            new EntityFieldDefinitionDraft(null, "model", _labels.ModelLabel, EntityFieldType.Text, 3, false, true, null, null, [])
        };

        if (entityType is null)
        {
            entityType = EntityType.Create(Guid.NewGuid(), name, code, description, fields);
            _dbContext.EntityTypes.Add(entityType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        entityType.Activate();

        return entityType;
    }

    private async Task EnsureInventoryItemAsync(
        EntityType entityType,
        Region region,
        Site site,
        string displayName,
        Guid createdBy,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.InventoryItems.AnyAsync(x => x.DisplayName == displayName, cancellationToken))
        {
            return;
        }

        _dbContext.InventoryItems.Add(InventoryItem.Create(
            Guid.NewGuid(),
            entityType,
            region,
            site,
            displayName,
            InventoryStatus.Operational,
            DateOnly.FromDateTime(createdAtUtc.Date),
            createdBy,
            createdAtUtc,
            [
                new InventoryAttributeValueDraft("serialNumber", $"{displayName}-SN"),
                new InventoryAttributeValueDraft("manufacturer", ResolveManufacturer(displayName)),
                new InventoryAttributeValueDraft("model", $"{entityType.Code.ToUpperInvariant()}-MVP")
            ]));
    }

    private async Task EnsureTemplateAsync(
        EntityType entityType,
        string name,
        string code,
        string description,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.PreventiveTemplates.AnyAsync(x => x.Code == code, cancellationToken))
        {
            return;
        }

        _dbContext.PreventiveTemplates.Add(PreventiveTemplate.Create(
            Guid.NewGuid(),
            entityType,
            name,
            code,
            description,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    _labels.VisualInspectionSection,
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(null, "equipmentClean", _labels.EquipmentCleanQuestion, PreventiveChecklistItemType.YesNo, 1, true, true, null, false, false, false, null, null, []),
                        new PreventiveChecklistItemDraft(null, "activeAlarm", _labels.ActiveAlarmQuestion, PreventiveChecklistItemType.YesNo, 2, true, true, null, true, true, false, null, null, [])
                    ]),
                new PreventiveTemplateSectionDraft(
                    null,
                    _labels.OperationalReadingsSection,
                    2,
                    true,
                    [
                        new PreventiveChecklistItemDraft(null, "operatingReading", _labels.OperatingReadingQuestion, PreventiveChecklistItemType.Numeric, 1, true, true, _labels.OperatingReadingHelpText, true, false, false, 1, 500, []),
                        new PreventiveChecklistItemDraft(
                            null,
                            "condition",
                            _labels.OverallConditionQuestion,
                            PreventiveChecklistItemType.Select,
                            2,
                            true,
                            true,
                            null,
                            true,
                            false,
                            false,
                            null,
                            null,
                            [
                                new PreventiveChecklistOptionDraft(null, "good", _labels.GoodOption, 1),
                                new PreventiveChecklistOptionDraft(null, "warning", _labels.WarningOption, 2),
                                new PreventiveChecklistOptionDraft(null, "critical", _labels.CriticalOption, 3)
                            ])
                    ])
            ]));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDemoExecutionsAsync(
        Guid technicianId,
        Guid validatorId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.PreventiveExecutions.AnyAsync(x => x.InventoryItemDisplayName == "UPS-01", cancellationToken))
        {
            return;
        }

        await CreateExecutionAsync("UPS-01", technicianId, null, now.AddDays(-2), cancellationToken);
        await CreateExecutionAsync("UPS-02", technicianId, "submitted", now.AddDays(-5), cancellationToken);
        await CreateExecutionAsync("GEN-01", technicianId, "approved", now.AddDays(-16), cancellationToken, validatorId, _labels.ReadingsAcceptedComment);
        await CreateExecutionAsync("GEN-02", technicianId, "rejected", now.AddDays(-24), cancellationToken, validatorId, _labels.GeneratorRejectedComment);
        await CreateExecutionAsync("HVAC-01", technicianId, "reworkRequested", now.AddDays(-10), cancellationToken, validatorId, _labels.HvacReworkComment);
        await CreateExecutionAsync("HVAC-02", technicianId, "approved", now.AddMonths(-1), cancellationToken, validatorId, _labels.HvacApprovedComment);
    }

    private async Task CreateExecutionAsync(
        string inventoryDisplayName,
        Guid technicianId,
        string? targetStatus,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken,
        Guid? validatorId = null,
        string? validationComment = null)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .Include(x => x.EntityType)
            .Include(x => x.Region)
            .Include(x => x.Site)
            .SingleAsync(x => x.DisplayName == inventoryDisplayName, cancellationToken);

        var template = await _dbContext.PreventiveTemplates
            .Include(x => x.Sections)
            .ThenInclude(x => x.ChecklistItems)
            .ThenInclude(x => x.Options)
            .OrderBy(x => x.Code)
            .FirstAsync(x => x.EntityTypeId == inventoryItem.EntityTypeId && x.IsActive, cancellationToken);

        var execution = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            inventoryItem,
            template,
            technicianId,
            startedAtUtc);

        if (targetStatus is null)
        {
            execution.UpdateDraft(
                [new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null)],
                technicianId,
                startedAtUtc.AddMinutes(12));
        }
        else
        {
            execution.Submit(CreateCompletedAnswers(targetStatus), technicianId, startedAtUtc.AddMinutes(35));

            if (targetStatus == "approved")
            {
                execution.Approve(validatorId!.Value, startedAtUtc.AddHours(4), validationComment);
            }
            else if (targetStatus == "rejected")
            {
                execution.Reject(validatorId!.Value, startedAtUtc.AddHours(4), validationComment!);
            }
            else if (targetStatus == "reworkRequested")
            {
                execution.RequestRework(validatorId!.Value, startedAtUtc.AddHours(4), validationComment!);
            }
        }

        _dbContext.PreventiveExecutions.Add(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IReadOnlyCollection<PreventiveExecutionAnswerDraft> CreateCompletedAnswers(string targetStatus)
    {
        var activeAlarmValue = targetStatus is "rejected" or "reworkRequested" ? "no" : "yes";
        var activeAlarmComment = activeAlarmValue == "no" ? _labels.AlarmConditionComment : null;
        var condition = targetStatus switch
        {
            "rejected" => "critical",
            "reworkRequested" => "warning",
            _ => "good"
        };

        return
        [
            new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null),
            new PreventiveExecutionAnswerDraft("activeAlarm", activeAlarmValue, activeAlarmComment),
            new PreventiveExecutionAnswerDraft("operatingReading", "220", null),
            new PreventiveExecutionAnswerDraft("condition", condition, null)
        ];
    }

    private static string ResolveManufacturer(string displayName)
    {
        if (displayName.StartsWith("GEN", StringComparison.OrdinalIgnoreCase))
        {
            return "Northwind Power";
        }

        if (displayName.StartsWith("HVAC", StringComparison.OrdinalIgnoreCase))
        {
            return "Contoso Cooling";
        }

        return "Fabrikam Energy";
    }
}
