using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Commands.CreateEntityType;
using InfraOps.Application.EntityTypes.Support;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;

namespace InfraOps.Application.Tests.EntityTypes.Commands.CreateEntityType;

public sealed class CreateEntityTypeCommandHandlerTests
{
    [Fact]
    public async Task Should_PersistEntityType_When_RequestIsValid()
    {
        var repository = new StubEntityTypeRepository();
        var unitOfWork = new StubUnitOfWork();
        var handler = new CreateEntityTypeCommandHandler(
            new CreateEntityTypeCommandValidator(),
            repository,
            unitOfWork);

        var result = await handler.Handle(
            new CreateEntityTypeCommand(
                "UPS",
                "ups",
                "Critical power backup assets.",
                [new EntityFieldDefinitionInput(null, "serialNumber", "Serial Number", "text", 1, true, true, null, null, [])]),
            CancellationToken.None);

        Assert.Equal("UPS", result.Name);
        Assert.Equal("ups", result.Code);
        Assert.NotNull(repository.AddedEntityType);
        Assert.True(unitOfWork.SaveChangesCalled);
    }

    [Fact]
    public async Task Should_RejectCreate_When_EntityTypeCodeAlreadyExists()
    {
        var handler = new CreateEntityTypeCommandHandler(
            new CreateEntityTypeCommandValidator(),
            new StubEntityTypeRepository { IsCodeInUse = true },
            new StubUnitOfWork());

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => handler.Handle(
            new CreateEntityTypeCommand("UPS", "ups", null, []),
            CancellationToken.None));

        Assert.Equal("Entity type code is already in use.", exception.Message);
    }

    private sealed class StubEntityTypeRepository : IEntityTypeRepository
    {
        public bool IsCodeInUse { get; set; }

        public EntityType? AddedEntityType { get; private set; }

        public Task AddAsync(EntityType entityType, CancellationToken cancellationToken)
        {
            AddedEntityType = entityType;
            return Task.CompletedTask;
        }

        public Task<EntityType?> GetByIdAsync(Guid entityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult<EntityType?>(null);
        }

        public Task<bool> IsCodeInUseAsync(string normalizedCode, Guid? excludedEntityTypeId, CancellationToken cancellationToken)
        {
            return Task.FromResult(IsCodeInUse);
        }

        public Task<IReadOnlyCollection<EntityType>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<EntityType>>([]);
        }

        public Task<IReadOnlyCollection<EntityType>> ListActiveAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<EntityType>>([]);
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
