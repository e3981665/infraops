using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveTemplates.Entities;

namespace InfraOps.Application.PreventiveTemplates.Commands.CreatePreventiveTemplate;

public sealed class CreatePreventiveTemplateCommandHandler : ICommandHandler<CreatePreventiveTemplateCommand, PreventiveTemplateDetailsDto>
{
    private readonly IValidator<CreatePreventiveTemplateCommand> _validator;
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePreventiveTemplateCommandHandler(
        IValidator<CreatePreventiveTemplateCommand> validator,
        IPreventiveTemplateRepository preventiveTemplateRepository,
        IEntityTypeRepository entityTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _preventiveTemplateRepository = preventiveTemplateRepository;
        _entityTypeRepository = entityTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PreventiveTemplateDetailsDto> Handle(
        CreatePreventiveTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entityType = await _entityTypeRepository.GetByIdAsync(command.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        var normalizedCode = PreventiveTemplateMappings.NormalizeTemplateCode(command.Code);

        if (await _preventiveTemplateRepository.IsCodeInUseAsync(normalizedCode, null, cancellationToken))
        {
            throw new DomainRuleException("Preventive template code is already in use.");
        }

        var preventiveTemplate = PreventiveTemplate.Create(
            Guid.NewGuid(),
            entityType,
            command.Name,
            command.Code,
            command.Description,
            command.Sections.Select(PreventiveTemplateMappings.ToDraft).ToArray());

        await _preventiveTemplateRepository.AddAsync(preventiveTemplate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdTemplate = await _preventiveTemplateRepository.GetByIdAsync(preventiveTemplate.Id, cancellationToken)
            ?? preventiveTemplate;

        return PreventiveTemplateMappings.ToDetailsDto(createdTemplate);
    }
}
