using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveValidations.Commands.RequestPreventiveRework;

public sealed record RequestPreventiveReworkCommand(
    Guid PreventiveExecutionId,
    string Reason)
    : ICommand<PreventiveExecutionDetailsDto>;
