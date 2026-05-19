using InfraOps.Application.Abstractions.Messaging;
using InfraOps.Application.PreventiveExecutions.Dtos;
using InfraOps.Application.PreventiveExecutions.Support;

namespace InfraOps.Application.PreventiveExecutions.Commands.SubmitPreventiveExecution;

public sealed record SubmitPreventiveExecutionCommand(
    Guid PreventiveExecutionId,
    IReadOnlyCollection<PreventiveExecutionAnswerInput>? Answers)
    : ICommand<PreventiveExecutionDetailsDto>;
