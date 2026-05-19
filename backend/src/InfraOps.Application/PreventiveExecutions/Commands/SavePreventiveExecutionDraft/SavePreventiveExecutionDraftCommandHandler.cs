using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Commands.SavePreventiveExecutionDraft;

public sealed class SavePreventiveExecutionDraftCommandHandler
    : ICommandHandler<SavePreventiveExecutionDraftCommand, PreventiveExecutionDetailsDto>
{
    private readonly IValidator<SavePreventiveExecutionDraftCommand> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public SavePreventiveExecutionDraftCommandHandler(
        IValidator<SavePreventiveExecutionDraftCommand> validator,
        IPreventiveExecutionRepository preventiveExecutionRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _preventiveExecutionRepository = preventiveExecutionRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<PreventiveExecutionDetailsDto> Handle(
        SavePreventiveExecutionDraftCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var execution = await _preventiveExecutionRepository.GetByIdAsync(command.PreventiveExecutionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive execution was not found.");

        execution.UpdateDraft(
            (command.Answers ?? []).Select(PreventiveExecutionMappings.ToDraft).ToArray(),
            currentUserId,
            _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PreventiveExecutionMappings.ToDetailsDto(execution);
    }
}
