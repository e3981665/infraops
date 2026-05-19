using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.EntityTypes.Abstractions;
using InfraOps.Application.PreventiveTemplates.Abstractions;
using InfraOps.Application.PreventiveTemplates.Dtos;
using InfraOps.Application.PreventiveTemplates.Support;
using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Application.PreventiveTemplates.Commands.UpdatePreventiveTemplate;

public sealed class UpdatePreventiveTemplateCommandHandler : ICommandHandler<UpdatePreventiveTemplateCommand, PreventiveTemplateDetailsDto>
{
    private readonly IValidator<UpdatePreventiveTemplateCommand> _validator;
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;
    private readonly IEntityTypeRepository _entityTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePreventiveTemplateCommandHandler(
        IValidator<UpdatePreventiveTemplateCommand> validator,
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
        UpdatePreventiveTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var preventiveTemplate = await _preventiveTemplateRepository.GetByIdAsync(command.PreventiveTemplateId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive template was not found.");
        var entityType = await _entityTypeRepository.GetByIdAsync(preventiveTemplate.EntityTypeId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Entity type was not found.");

        var normalizedCode = PreventiveTemplateMappings.NormalizeTemplateCode(command.Code);

        if (await _preventiveTemplateRepository.IsCodeInUseAsync(normalizedCode, command.PreventiveTemplateId, cancellationToken))
        {
            throw new DomainRuleException("Preventive template code is already in use.");
        }

        preventiveTemplate.Update(
            entityType,
            command.Name,
            command.Code,
            command.Description,
            command.Sections.Select(PreventiveTemplateMappings.ToDraft).ToArray());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedTemplate = await _preventiveTemplateRepository.GetByIdAsync(command.PreventiveTemplateId, cancellationToken)
            ?? preventiveTemplate;

        return PreventiveTemplateMappings.ToDetailsDto(updatedTemplate);
    }
}
