using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Support;
using InfraOps.Application.PreventiveValidations.Commands.ApprovePreventiveExecution;
using InfraOps.Application.PreventiveValidations.Commands.RejectPreventiveExecution;
using InfraOps.Application.PreventiveValidations.Commands.RequestPreventiveRework;
using InfraOps.Application.PreventiveValidations.Queries.GetPreventiveValidationDetail;
using InfraOps.Application.PreventiveValidations.Queries.ListPreventiveValidations;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.Inventory.Entities;
using InfraOps.Domain.Inventory.Enums;
using InfraOps.Domain.Inventory.Models;
using InfraOps.Domain.Locations.Entities;
using InfraOps.Domain.PreventiveExecutions.Entities;
using InfraOps.Domain.PreventiveExecutions.Enums;
using InfraOps.Domain.PreventiveExecutions.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Application.Tests.PreventiveValidations;

public sealed class PreventiveValidationUseCaseTests
{
    [Fact]
    public async Task Should_ListSubmittedExecutionsForValidation()
    {
        var submitted = CreateSubmittedExecution("UPS-01");
        var draft = CreateDraftExecution("UPS-02");
        var handler = new ListPreventiveValidationsQueryHandler(
            new ListPreventiveValidationsQueryValidator(),
            new StubPreventiveExecutionRepository(submitted, draft));

        var result = await handler.Handle(
            new ListPreventiveValidationsQuery(null, null, null, null, null, null, null, null, "UPS"),
            CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(submitted.Id, result.Single().Id);
    }

    [Fact]
    public async Task Should_ReturnValidationDetail()
    {
        var submitted = CreateSubmittedExecution("UPS-01");
        var handler = new GetPreventiveValidationDetailQueryHandler(
            new StubPreventiveExecutionRepository(submitted));

        var result = await handler.Handle(new GetPreventiveValidationDetailQuery(submitted.Id), CancellationToken.None);

        Assert.Equal(submitted.Id, result.Id);
        Assert.Equal("submitted", result.Status);
        Assert.NotEmpty(result.TemplateSections);
        Assert.NotEmpty(result.Answers);
    }

    [Fact]
    public async Task Should_ApproveThroughUseCase()
    {
        var submitted = CreateSubmittedExecution("UPS-01");
        var handler = new ApprovePreventiveExecutionCommandHandler(
            new ApprovePreventiveExecutionCommandValidator(),
            new StubPreventiveExecutionRepository(submitted),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new ApprovePreventiveExecutionCommand(submitted.Id, null),
            CancellationToken.None);

        Assert.Equal("approved", result.Status);
        Assert.Single(result.ValidationHistory);
    }

    [Fact]
    public async Task Should_RejectThroughUseCase()
    {
        var submitted = CreateSubmittedExecution("UPS-01");
        var handler = new RejectPreventiveExecutionCommandHandler(
            new RejectPreventiveExecutionCommandValidator(),
            new StubPreventiveExecutionRepository(submitted),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new RejectPreventiveExecutionCommand(submitted.Id, "Measurements are outside policy."),
            CancellationToken.None);

        Assert.Equal("rejected", result.Status);
        Assert.Equal("Measurements are outside policy.", result.ValidationHistory.Single().Comment);
    }

    [Fact]
    public async Task Should_RequestReworkThroughUseCase()
    {
        var submitted = CreateSubmittedExecution("UPS-01");
        var handler = new RequestPreventiveReworkCommandHandler(
            new RequestPreventiveReworkCommandValidator(),
            new StubPreventiveExecutionRepository(submitted),
            new StubCurrentUser(),
            new StubClock(),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new RequestPreventiveReworkCommand(submitted.Id, "Clarify alarm comment."),
            CancellationToken.None);

        Assert.Equal("reworkRequested", result.Status);
        Assert.Equal("Clarify alarm comment.", result.ValidationHistory.Single().Comment);
    }

    private sealed class StubPreventiveExecutionRepository : IPreventiveExecutionRepository
    {
        private readonly List<PreventiveExecution> _executions;

        public StubPreventiveExecutionRepository(params PreventiveExecution[] executions)
        {
            _executions = executions.ToList();
        }

        public Task AddAsync(PreventiveExecution preventiveExecution, CancellationToken cancellationToken)
        {
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

            if (filter.SiteId.HasValue)
            {
                query = query.Where(x => x.SiteId == filter.SiteId.Value);
            }

            if (filter.RegionId.HasValue)
            {
                query = query.Where(x => x.RegionId == filter.RegionId.Value);
            }

            if (filter.SubmittedBy.HasValue)
            {
                query = query.Where(x => x.SubmittedBy == filter.SubmittedBy.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => x.InventoryItemDisplayName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyCollection<PreventiveExecution>>(query.ToArray());
        }
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

    private static PreventiveExecution CreateSubmittedExecution(string displayName)
    {
        var execution = CreateDraftExecution(displayName);
        execution.Submit(
            [
                new PreventiveExecutionAnswerDraft("equipmentClean", "yes", null),
                new PreventiveExecutionAnswerDraft("activeAlarm", "yes", null)
            ],
            StubCurrentUser.UserGuid,
            StubClock.Now);

        return execution;
    }

    private static PreventiveExecution CreateDraftExecution(string displayName)
    {
        return PreventiveExecution.CreateDraft(
            Guid.NewGuid(),
            CreateInventoryItem(displayName),
            CreateTemplate(),
            StubCurrentUser.UserGuid,
            StubClock.Now);
    }

    private static InventoryItem CreateInventoryItem(string displayName)
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
