using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Commands.CreatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Application.Tests.PreventiveTemplates.Commands.CreatePreventiveTemplate;

public sealed class CreatePreventiveTemplateCommandHandlerTests
{
    [Fact]
    public async Task Should_PersistValidPreventiveTemplate_When_RequestIsValid()
    {
        var entityType = CreateEntityType();
        var repository = new StubPreventiveTemplateRepository();
        var unitOfWork = new StubUnitOfWork();
        var handler = new CreatePreventiveTemplateCommandHandler(
            new CreatePreventiveTemplateCommandValidator(),
            repository,
            new StubEntityTypeRepository(entityType),
            unitOfWork);

        var result = await handler.Handle(
            new CreatePreventiveTemplateCommand(
                entityType.Id,
                "UPS Quarterly Inspection",
                "ups-quarterly-inspection",
                "Quarterly preventive checklist for UPS assets.",
                [new PreventiveTemplateSectionInput(
                    null,
                    "Visual Inspection",
                    1,
                    true,
                    [new PreventiveChecklistItemInput(
                        null,
                        "equipmentClean",
                        "Equipment clean?",
                        "yesNo",
                        1,
                        true,
                        true,
                        null,
                        false,
                        false,
                        false,
                        null,
                        null,
                        [])])]),
            CancellationToken.None);

        Assert.Equal("UPS Quarterly Inspection", result.Name);
        Assert.NotNull(repository.AddedPreventiveTemplate);
        Assert.True(unitOfWork.SaveChangesCalled);
        Assert.Single(repository.AddedPreventiveTemplate!.Sections);
    }

    [Fact]
    public async Task Should_RejectCreation_When_EntityTypeIsInactive()
    {
        var inactiveEntityType = CreateEntityType();
        inactiveEntityType.Deactivate();

        var handler = new CreatePreventiveTemplateCommandHandler(
            new CreatePreventiveTemplateCommandValidator(),
            new StubPreventiveTemplateRepository(),
            new StubEntityTypeRepository(inactiveEntityType),
            new StubUnitOfWork());

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => handler.Handle(
            new CreatePreventiveTemplateCommand(
                inactiveEntityType.Id,
                "UPS Quarterly Inspection",
                "ups-quarterly-inspection",
                null,
                []),
            CancellationToken.None));

        Assert.Equal("Preventive templates must belong to an active entity type.", exception.Message);
    }

    [Fact]
    public async Task Should_RejectCreation_When_TemplateCodeAlreadyExists()
    {
        var handler = new CreatePreventiveTemplateCommandHandler(
            new CreatePreventiveTemplateCommandValidator(),
            new StubPreventiveTemplateRepository { IsCodeInUse = true },
            new StubEntityTypeRepository(CreateEntityType()),
            new StubUnitOfWork());

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => handler.Handle(
            new CreatePreventiveTemplateCommand(
                Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
                "UPS Quarterly Inspection",
                "ups-quarterly-inspection",
                null,
                []),
            CancellationToken.None));

        Assert.Equal("Preventive template code is already in use.", exception.Message);
    }

    private static EntityType CreateEntityType()
    {
        return EntityType.Create(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups",
            null,
            [new EntityFieldDefinitionDraft(
                null,
                "serialNumber",
                "Serial Number",
                EntityFieldType.Text,
                1,
                true,
                true,
                null,
                null,
                [])]);
    }

    private sealed class StubPreventiveTemplateRepository : IPreventiveTemplateRepository
    {
        public bool IsCodeInUse { get; set; }

        public PreventiveTemplate? AddedPreventiveTemplate { get; private set; }

        public Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken)
        {
            AddedPreventiveTemplate = preventiveTemplate;
            return Task.CompletedTask;
        }

        public Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken)
        {
            return Task.FromResult(AddedPreventiveTemplate);
        }

        public Task<bool> IsCodeInUseAsync(
            string normalizedCode,
            Guid? excludedPreventiveTemplateId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(IsCodeInUse);
        }

        public Task<IReadOnlyCollection<PreventiveTemplate>> ListAsync(
            PreventiveTemplateListFilter filter,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PreventiveTemplate>>([]);
        }

        public Task<IReadOnlyCollection<PreventiveTemplate>> ListByEntityTypeAsync(
            Guid entityTypeId,
            bool? isActive,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<PreventiveTemplate>>([]);
        }
    }

    private sealed class StubEntityTypeRepository : IEntityTypeRepository
    {
        private readonly EntityType _entityType;

        public StubEntityTypeRepository(EntityType entityType)
        {
            _entityType = entityType;
        }

        public Task AddAsync(EntityType entityType, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult<EntityType?>(_entityType);
        }

        public Task<bool> IsCodeInUseAsync(string normalizedCode, Guid? excludedEntityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<EntityType>>([]);
        }

        public Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<EntityType>>(_entityType.IsActive ? [_entityType] : []);
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public bool SaveChangesCalled { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }
}
