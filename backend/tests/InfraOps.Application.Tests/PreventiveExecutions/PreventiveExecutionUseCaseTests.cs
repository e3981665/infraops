using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.Inventory.Abstractions;
using InfraOps.Application.Inventory.Support;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Commands.StartPreventiveExecution;
using InfraOps.Application.PreventiveExecutions.Commands.SubmitPreventiveExecution;
using InfraOps.Application.PreventiveExecutions.Queries.ListPreventiveExecutions;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Application.Tests.PreventiveExecutions;

public sealed class PreventiveExecutionUseCaseTests
{
    [Fact]
    public async Task Should_PersistValidDraftExecution_When_StartRequestIsValid()
    {
        var repository = new StubPreventiveExecutionRepository();
        var inventoryItem = CreateInventoryItem();
        var template = CreateTemplate();
        var handler = new StartPreventiveExecutionCommandHandler(
            new StartPreventiveExecutionCommandValidator(),
            repository,
            new StubInventoryItemRepository(inventoryItem),
            new StubPreventiveTemplateRepository([template]),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(new StartPreventiveExecutionCommand(inventoryItem.Id), CancellationToken.None);

        Assert.Equal("draft", result.Status);
        Assert.NotNull(repository.AddedExecution);
        Assert.Equal(inventoryItem.Id, repository.AddedExecution!.InventoryItemId);
        Assert.NotEmpty(result.TemplateSections);
    }

    [Fact]
    public async Task Should_SubmitValidExecutionCorrectly()
    {
        var execution = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem(),
            CreateTemplate(),
            StubCurrentUser.UserGuid,
            StubClock.Now);
        var repository = new StubPreventiveExecutionRepository(execution);
        var handler = new SubmitPreventiveExecutionCommandHandler(
            new SubmitPreventiveExecutionCommandValidator(),
            repository,
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new SubmitPreventiveExecutionCommand(
                execution.Id,
                [
                    new PreventiveExecutionAnswerInput("equipmentClean", "yes", null),
                    new PreventiveExecutionAnswerInput("activeAlarm", "yes", null)
                ]),
            CancellationToken.None);

        Assert.Equal("submitted", result.Status);
        Assert.NotNull(result.SubmittedAtUtc);
    }

    [Fact]
    public async Task Should_ListExecutionsWithFilters()
    {
        var matching = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem("UPS-01"),
            CreateTemplate(),
            StubCurrentUser.UserGuid,
            StubClock.Now);
        var other = PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem("UPS-02"),
            CreateTemplate(),
            StubCurrentUser.UserGuid,
            StubClock.Now);
        var handler = new ListPreventiveExecutionsQueryHandler(
            new ListPreventiveExecutionsQueryValidator(),
            new StubPreventiveExecutionRepository(matching, other),
            new StubCurrentUser());

        var result = await handler.Handle(
            new ListPreventiveExecutionsQuery(
                "draft",
                matching.EntityTypeId,
                matching.InventoryItemId,
                null,
                null,
                false,
                null,
                null,
                "UPS-01"),
            CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("UPS-01", result.Single().InventoryItemDisplayName);
    }

    private sealed class StubPreventiveExecutionRepository : IPreventiveExecutionRepository
    {
        private readonly List<PreventiveExecution> _executions;

        public StubPreventiveExecutionRepository(params PreventiveExecution[] executions)
        {
            _executions = executions.ToList();
        }

        public PreventiveExecution? AddedExecution { get; private set; }

        public Task AddAsync(PreventiveExecution preventiveExecution, CancellationToken cancellationToken)
        {
            AddedExecution = preventiveExecution;
            _executions.Add(preventiveExecution);
            return Task.CompletedTask;
        }

        public Task<PreventiveExecution?> GetByIdAsync(Guid preventiveExecutionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_executions.SingleOrDefault(x => x.Id == preventiveExecutionId));
        }

        public Task<IReadOnlyCollection<PreventiveExecution>> ListAsync(
            PreventiveExecutionListFilter filter,
            CancellationToken cancellationToken)
        {
            IEnumerable<PreventiveExecution> query = _executions;

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.EntityTypeId.HasValue)
            {
                query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
            }

            if (filter.InventoryItemId.HasValue)
            {
                query = query.Where(x => x.InventoryItemId == filter.InventoryItemId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => x.InventoryItemDisplayName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyCollection<PreventiveExecution>>(query.ToArray());
        }
    }

    private sealed class StubInventoryItemRepository : IInventoryItemRepository
    {
        private readonly InventoryItem _inventoryItem;

        public StubInventoryItemRepository(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
        }

        public Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<InventoryItem?> GetByIdAsync(Guid inventoryItemId, CancellationToken cancellationToken)
            => Task.FromResult<InventoryItem?>(_inventoryItem);

        public Task<IReadOnlyCollection<InventoryItem>> ListAsync(InventoryListFilter filter, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<InventoryItem>>([_inventoryItem]);
    }

    private sealed class StubPreventiveTemplateRepository : IPreventiveTemplateRepository
    {
        private readonly IReadOnlyCollection<PreventiveTemplate> _templates;

        public StubPreventiveTemplateRepository(IReadOnlyCollection<PreventiveTemplate> templates)
        {
            _templates = templates;
        }

        public Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken)
            => Task.FromResult(_templates.SingleOrDefault(x => x.Id == preventiveTemplateId));

        public Task<IReadOnlyCollection<PreventiveTemplate>> ListAsync(PreventiveTemplateListFilter filter, CancellationToken cancellationToken)
            => Task.FromResult(_templates);

        public Task<IReadOnlyCollection<PreventiveTemplate>> ListByEntityTypeAsync(
            Guid entityTypeId,
            bool? isActive,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PreventiveTemplate>>(
                _templates.Where(x => x.EntityTypeId == entityTypeId && (!isActive.HasValue || x.IsActive == isActive.Value)).ToArray());
        }

        public Task<bool> IsCodeInUseAsync(string normalizedCode, Guid? excludedPreventiveTemplateId, CancellationToken cancellationToken)
            => Task.FromResult(false);
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        public static Guid UserGuid => Guid.Parse("1B84A5A9-E7FB-4F66-965E-B75A1F0D18C4");

        public bool IsAuthenticated => true;

        public Guid? UserId => UserGuid;
    }

    private sealed class StubClock : IClock
    {
        public static DateTimeOffset Now => new(2026, 4, 2, 12, 0, 0, TimeSpan.Zero);

        public DateTimeOffset UtcNow => Now;
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static InventoryItem CreateInventoryItem(string displayName = "UPS-01")
    {
        var region = new Region(Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115"), "north-region", "North Region");
        var site = new Site(Guid.Parse("720C4A9A-94BF-47B8-A1CF-24F346955F7E"), region.Id, "north-hub", "North Hub");

        return InventoryItem.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            region,
            site,
            displayName,
            InventoryStatus.Operational,
            null,
            StubCurrentUser.UserGuid,
            StubClock.Now,
            [new InventoryAttributeValueDraft("serialNumber", $"{displayName}-serial")]);
    }

    private static PreventiveTemplate CreateTemplate()
    {
        return PreventiveTemplate.Create(
            Guid.NewGuid(),
            CreateEntityType(),
            "Quarterly UPS Inspection",
            "quarterly-ups-inspection",
            null,
            [
                new PreventiveTemplateSectionDraft(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [
                        new PreventiveChecklistItemDraft(null, "equipmentClean", "Equipment clean?", PreventiveChecklistItemType.YesNo, 1, true, true, null, false, false, false, null, null, []),
                        new PreventiveChecklistItemDraft(null, "activeAlarm", "Any active alarm?", PreventiveChecklistItemType.YesNo, 2, true, true, null, true, true, false, null, null, [])
                    ])
            ]);
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups",
            null,
            [new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, [])]);
    }
}
