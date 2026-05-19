using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Commands.SubmitPreventiveExecution;

public sealed class SubmitPreventiveExecutionCommandHandler
    : ICommandHandler<SubmitPreventiveExecutionCommand, PreventiveExecutionDetailsDto>
{
    private readonly IValidator<SubmitPreventiveExecutionCommand> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPreventiveExecutionCommandHandler(
        IValidator<SubmitPreventiveExecutionCommand> validator,
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
        SubmitPreventiveExecutionCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var execution = await _preventiveExecutionRepository.GetByIdAsync(command.PreventiveExecutionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive execution was not found.");

        execution.Submit(
            command.Answers?.Select(PreventiveExecutionMappings.ToDraft).ToArray(),
            currentUserId,
            _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PreventiveExecutionMappings.ToDetailsDto(execution);
    }
}
