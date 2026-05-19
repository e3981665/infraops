using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Support;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.Entities;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Application.EntityTypes.Commands.CreateEntityType;

public sealed class CreateEntityTypeCommandHandler : ICommandHandler<CreateEntityTypeCommand, EntityTypeDetailsDto>
{
    private readonly IValidator<CreateEntityTypeCommand> _validator;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEntityTypeCommandHandler(
        IValidator<CreateEntityTypeCommand> validator,
        IEntityTypeRepository entityTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _entityTypeRepository = entityTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityTypeDetailsDto> Handle(
        CreateEntityTypeCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedCode = EntityTypeCode.Create(command.Code).Value;

        if (await _entityTypeRepository.IsCodeInUseAsync(normalizedCode, null, cancellationToken))
        {
            throw new DomainRuleException("Entity type code is already in use.");
        }

        var entityType = EntityType.Create(
            Guid.NewGuid(),
            command.Name,
            command.Code,
            command.Description,
            command.FieldDefinitions.Select(EntityTypeMappings.ToDraft).ToArray());

        await _entityTypeRepository.AddAsync(entityType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityTypeMappings.ToDetailsDto(entityType);
    }
}
