namespace InfraOps.Api.Contracts.Requests.PreventiveExecutions;

public sealed record SubmitPreventiveExecutionRequest(
    IReadOnlyCollection<PreventiveExecutionAnswerRequest>? Answers);
