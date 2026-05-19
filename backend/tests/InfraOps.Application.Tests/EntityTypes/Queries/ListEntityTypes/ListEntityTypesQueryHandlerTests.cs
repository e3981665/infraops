using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Queries.ListEntityTypes;
using InfraOps.Domain.EntityTypes.Entities;

namespace InfraOps.Application.Tests.EntityTypes.Queries.ListEntityTypes;

public sealed class ListEntityTypesQueryHandlerTests
{
    [Fact]
    public async Task Should_ListEntityTypes()
    {
        var handler = new ListEntityTypesQueryHandler(new StubEntityTypeRepository(
            EntityType.Create(Guid.NewGuid(), "Generator", "generator", null, []),
            EntityType.Create(Guid.NewGuid(), "UPS", "ups", null, [])));

        var result = await handler.Handle(new ListEntityTypesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Generator", result.First().Name);
        Assert.Equal("UPS", result.Last().Name);
    }

    private sealed class StubEntityTypeRepository : IEntityTypeRepository
    {
        private readonly IReadOnlyCollection<EntityType> _entityTypes;

        public StubEntityTypeRepository(params EntityType[] entityTypes)
        {
            _entityTypes = entityTypes;
        }

        public Task AddAsync(EntityType entityType, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult<EntityType?>(null);
        }

        public Task<bool> IsCodeInUseAsync(string normalizedCode, Guid? excludedEntityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_entityTypes);
        }

        public Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_entityTypes);
        }
    }
}
