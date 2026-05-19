using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveValidations.Commands.ApprovePreventiveExecution;

public sealed record ApprovePreventiveExecutionCommand(
    Guid PreventiveExecutionId,
    string? Comment)
    : ICommand<PreventiveExecutionDetailsDto>;
