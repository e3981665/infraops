using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Queries.ListPreventiveTemplates;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;
using InfraOps.Domain.PreventiveTemplates.Entities;
using InfraOps.Domain.PreventiveTemplates.Enums;
using InfraOps.Domain.PreventiveTemplates.Models;

namespace InfraOps.Application.Tests.PreventiveTemplates.Queries.ListPreventiveTemplates;

public sealed class ListPreventiveTemplatesQueryHandlerTests
{
    [Fact]
    public async Task Should_ListPreventiveTemplatesWithFilters_When_RequestMatchesExistingTemplates()
    {
        var upsEntityType = CreateEntityType(
            Guid.Parse("26043C08-0880-46D9-B7DC-5778D07D64A9"),
            "UPS",
            "ups");
        var generatorEntityType = CreateEntityType(
            Guid.Parse("C295E7FA-27A8-4D4D-B6B8-E6B8C8FFBE28"),
            "Generator",
            "generator");

        var matchingTemplate = PreventiveTemplate.Create(
            Guid.Parse("AB711710-4114-4F2C-B7AB-E64F930B5B92"),
            upsEntityType,
            "UPS Quarterly Inspection",
            "ups-quarterly-inspection",
            "Quarterly checklist.",
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

        var nonMatchingTemplate = PreventiveTemplate.Create(
            Guid.Parse("4F3B005A-A2EE-4AA5-AFFD-F1E039E95B8A"),
            generatorEntityType,
            "Generator Fuel Check",
            "generator-fuel-check",
            "Generator checklist.",
            []);

        var handler = new ListPreventiveTemplatesQueryHandler(
            new ListPreventiveTemplatesQueryValidator(),
            new StubPreventiveTemplateRepository([matchingTemplate, nonMatchingTemplate]));

        var result = await handler.Handle(
            new ListPreventiveTemplatesQuery(upsEntityType.Id, true, "Quarterly"),
            CancellationToken.None);

        var template = Assert.Single(result);
        Assert.Equal(matchingTemplate.Id, template.Id);
        Assert.Equal("UPS Quarterly Inspection", template.Name);
        Assert.Equal(1, template.SectionCount);
        Assert.Equal(1, template.ChecklistItemCount);
    }

    private static EntityType CreateEntityType(Guid entityTypeId, string name, string code)
    {
        return EntityType.Create(
            entityTypeId,
            name,
            code,
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
        private readonly IReadOnlyCollection<PreventiveTemplate> _preventiveTemplates;

        public StubPreventiveTemplateRepository(IReadOnlyCollection<PreventiveTemplate> preventiveTemplates)
        {
            _preventiveTemplates = preventiveTemplates;
        }

        public Task AddAsync(PreventiveTemplate preventiveTemplate, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("AddAsync is not used by list query tests.");
        }

        public Task<PreventiveTemplate?> GetByIdAsync(Guid preventiveTemplateId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_preventiveTemplates.SingleOrDefault(x => x.Id == preventiveTemplateId));
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
            IEnumerable<PreventiveTemplate> query = _preventiveTemplates;

            if (filter.EntityTypeId.HasValue)
            {
                query = query.Where(x => x.EntityTypeId == filter.EntityTypeId.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.Name.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    x.Code.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyCollection<PreventiveTemplate>>(query.ToArray());
        }

        public Task<IReadOnlyCollection<PreventiveTemplate>> ListByEntityTypeAsync(
            Guid entityTypeId,
            bool? isActive,
            CancellationToken cancellationToken)
        {
            IEnumerable<PreventiveTemplate> query = _preventiveTemplates.Where(x => x.EntityTypeId == entityTypeId);

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            return Task.FromResult<IReadOnlyCollection<PreventiveTemplate>>(query.ToArray());
        }
    }
}
