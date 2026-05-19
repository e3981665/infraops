using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Commands.UpdateEntityType;
using InfraOps.Application.EntityTypes.Support;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.Enums;
using InfraOps.Domain.EntityTypes.Models;

namespace InfraOps.Application.Tests.EntityTypes.Commands.UpdateEntityType;

public sealed class UpdateEntityTypeCommandHandlerTests
{
    [Fact]
    public async Task Should_UpdateEntityTypeCorrectly_When_RequestIsValid()
    {
        var entityType = EntityType.Create(
            Guid.NewGuid(),
            "UPS",
            "ups",
            null,
            [
                new EntityFieldDefinitionDraft(null, "serialNumber", "Serial Number", EntityFieldType.Text, 1, true, true, null, null, []),
                new EntityFieldDefinitionDraft(null, "activeAlarm", "Active Alarm", EntityFieldType.Boolean, 2, false, true, null, null, [])
            ]);

        var serialNumberField = entityType.FieldDefinitions.Single(x => x.FieldKey == "serialNumber");
        var activeAlarmField = entityType.FieldDefinitions.Single(x => x.FieldKey == "activeAlarm");
        var handler = new UpdateEntityTypeCommandHandler(
            new UpdateEntityTypeCommandValidator(),
            new StubEntityTypeRepository(entityType),
            new StubUnitOfWork());

        var result = await handler.Handle(
            new UpdateEntityTypeCommand(
                entityType.Id,
                "UPS System",
                "ups-system",
                "Updated description",
                [
                    new EntityFieldDefinitionInput(
                        serialNumberField.Id,
                        serialNumberField.FieldKey,
                        "Asset Serial Number",
                        "text",
                        1,
                        true,
                        true,
                        null,
                        "Shown on the inventory registration form.",
                        [])
                ]),
            CancellationToken.None);

        Assert.Equal("UPS System", result.Name);
        Assert.Equal("ups-system", result.Code);
        Assert.Contains(result.FieldDefinitions, x => x.FieldKey == "serialNumber" && x.DisplayLabel == "Asset Serial Number");
        Assert.Contains(result.FieldDefinitions, x => x.FieldKey == "activeAlarm" && !x.IsActive);
        Assert.False(activeAlarmField.IsActive);
    }

    private sealed class StubEntityTypeRepository : IEntityTypeRepository
    {
        private readonly EntityType _entityType;

        public StubEntityTypeRepository(EntityType entityType)
        {
            _entityType = entityType;
        }

        public Task AddAsync(EntityType entityType, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

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
            return Task.FromResult<IReadOnlyCollection<EntityType>>([]);
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
