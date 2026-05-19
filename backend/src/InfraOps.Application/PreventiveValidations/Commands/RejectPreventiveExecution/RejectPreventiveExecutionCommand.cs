using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveValidations.Commands.RejectPreventiveExecution;

public sealed record RejectPreventiveExecutionCommand(
    Guid PreventiveExecutionId,
    string Reason)
    : ICommand<PreventiveExecutionDetailsDto>;
