namespace InfraOps.Api.Contracts.Requests.PreventiveExecutions;

public sealed record SavePreventiveExecutionDraftRequest(
    IReadOnlyCollection<PreventiveExecutionAnswerRequest> Answers);
