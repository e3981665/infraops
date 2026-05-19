using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Commands.UpdatePreventiveTemplate;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Application.Tests.PreventiveTemplates.Commands.UpdatePreventiveTemplate;

public sealed class UpdatePreventiveTemplateCommandHandlerTests
{
    [Fact]
    public async Task Should_UpdatePreventiveTemplate_When_RequestIsValid()
    {
        var entityType = CreateEntityType();
        var preventiveTemplate = PreventiveTemplate.Create(
            Guid.Parse("B06F693E-4794-435E-82D1-B6285E0A8397"),
            entityType,
            "UPS Quarterly Inspection",
            "ups-quarterly-inspection",
            "Quarterly preventive checklist.",
            [new PreventiveTemplateSectionDraft(
                null,
                "Visual Inspection",
                1,
                true,
                [new PreventiveChecklistItemDraft(
                    null,
                    "equipmentClean",
                    "Equipment clean?",
                    PreventiveChecklistItemType.YesNo,
                    1,
                    true,
                    true,
                    null,
                    false,
                    false,
                    false,
                    null,
                    null,
                    [])])]);

        var repository = new StubPreventiveTemplateRepository(preventiveTemplate);
        var handler = new UpdatePreventiveTemplateCommandHandler(
            new UpdatePreventiveTemplateCommandValidator(),
            repository,
            new StubEntityTypeRepository(entityType),
            new StubUnitOfWork());

        var existingSection = preventiveTemplate.Sections.Single();
        var existingItem = existingSection.ChecklistItems.Single();

        var result = await handler.Handle(
            new UpdatePreventiveTemplateCommand(
                preventiveTemplate.Id,
                "UPS Semiannual Inspection",
                "ups-semiannual-inspection",
                "Updated description.",
                [new PreventiveTemplateSectionInput(
                    existingSection.Id,
                    "Electrical Measurements",
                    1,
                    true,
                    [new PreventiveChecklistItemInput(
                        existingItem.Id,
                        existingItem.ItemKey,
                        "Input voltage",
                        "numeric",
                        1,
                        true,
                        true,
                        "Record the measured voltage.",
                        false,
                        false,
                        false,
                        210,
                        240,
                        [])])]),
            CancellationToken.None);

        Assert.Equal("UPS Semiannual Inspection", result.Name);
        Assert.Equal("ups-semiannual-inspection", result.Code);
        Assert.Single(result.Sections);
        Assert.Equal("Electrical Measurements", result.Sections.Single().Title);
        Assert.Equal(210, result.Sections.Single().ChecklistItems.Single().MinimumValue);
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
        private readonly PreventiveTemplate _preventiveTemplate;

        public StubPreventiveTemplateRepository(PreventiveTemplate preventiveTemplate)
        {
            _preventiveTemplate = preventiveTemplate;
        }

        public Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("AddAsync is not used by update tests.");
        }

        public Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken)
        {
            return Task.FromResult<PreventiveTemplate?>(_preventiveTemplate);
        }

        public Task<bool> IsCodeInUseAsync(
            string normalizedCode,
            Guid? excludedPreventiveTemplateId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
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
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
