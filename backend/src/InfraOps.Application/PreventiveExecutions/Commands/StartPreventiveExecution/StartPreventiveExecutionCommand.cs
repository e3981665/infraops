using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;

namespace InfraOps.Application.PreventiveExecutions.Commands.StartPreventiveExecution;

public sealed record StartPreventiveExecutionCommand(Guid InventoryItemId)
    : ICommand<PreventiveExecutionDetailsDto>;
