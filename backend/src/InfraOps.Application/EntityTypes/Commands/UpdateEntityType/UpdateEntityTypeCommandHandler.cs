using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.EntityTypes.Dtos;
using InfraOps.Application.EntityTypes.Support;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.EntityTypes.ValueObjects;

namespace InfraOps.Application.EntityTypes.Commands.UpdateEntityType;

public sealed class UpdateEntityTypeCommandHandler : ICommandHandler<UpdateEntityTypeCommand, EntityTypeDetailsDto>
{
    private readonly IValidator<UpdateEntityTypeCommand> _validator;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEntityTypeCommandHandler(
        IValidator<UpdateEntityTypeCommand> validator,
        IEntityTypeRepository entityTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _entityTypeRepository = entityTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityTypeDetailsDto> Handle(
        UpdateEntityTypeCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entityType = await _entityTypeRepository.GetByIdAsync(command.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        var normalizedCode = EntityTypeCode.Create(command.Code).Value;

        if (await _entityTypeRepository.IsCodeInUseAsync(normalizedCode, command.EntityTypeId, cancellationToken))
        {
            throw new DomainRuleException("Entity type code is already in use.");
        }

        entityType.Update(
            command.Name,
            command.Code,
            command.Description,
            command.FieldDefinitions.Select(EntityTypeMappings.ToDraft).ToArray());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityTypeMappings.ToDetailsDto(entityType);
    }
}
