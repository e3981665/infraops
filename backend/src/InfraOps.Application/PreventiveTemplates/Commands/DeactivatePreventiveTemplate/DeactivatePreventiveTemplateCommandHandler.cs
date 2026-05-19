using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.PreventiveTemplates.Abstractions;

namespace InfraOps.Application.PreventiveTemplates.Commands.DeactivatePreventiveTemplate;

public sealed class DeactivatePreventiveTemplateCommandHandler : ICommandHandler<DeactivatePreventiveTemplateCommand>
{
    private readonly IPreventiveTemplateRepository _preventiveTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePreventiveTemplateCommandHandler(
        IPreventiveTemplateRepository preventiveTemplateRepository,
        IUnitOfWork unitOfWork)
    {
        _preventiveTemplateRepository = preventiveTemplateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivatePreventiveTemplateCommand command, CancellationToken cancellationToken)
    {
        var preventiveTemplate = await _preventiveTemplateRepository.GetByIdAsync(command.PreventiveTemplateId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive template was not found.");

        preventiveTemplate.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
