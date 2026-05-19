using FluentValidation;
using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.Abstractions.Persistence;
using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Application.Identity.Abstractions;
using InfraOps.Application.PreventiveExecutions.Abstractions;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveValidations.Commands.ApprovePreventiveExecution;

public sealed class ApprovePreventiveExecutionCommandHandler
    : ICommandHandler<ApprovePreventiveExecutionCommand, PreventiveExecutionDetailsDto>
{
    private readonly IValidator<ApprovePreventiveExecutionCommand> _validator;
    private readonly IPreventiveExecutionRepository _preventiveExecutionRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePreventiveExecutionCommandHandler(
        IValidator<ApprovePreventiveExecutionCommand> validator,
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
        ApprovePreventiveExecutionCommand command,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var currentUserId = _currentUser.UserId
            ?? throw new ApplicationUnauthorizedException("Authenticated user context is required.");

        var execution = await _preventiveExecutionRepository.GetByIdAsync(command.PreventiveExecutionId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Preventive execution was not found.");

        execution.Approve(currentUserId, _clock.UtcNow, command.Comment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PreventiveExecutionMappings.ToDetailsDto(execution);
    }
}
