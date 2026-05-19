using InfraOps.Domain.Common.Exceptions;
using InfraOps.Domain.PreventiveExecutions.Enums;

namespace InfraOps.Domain.PreventiveExecutions.Entities;

public sealed class PreventiveValidationRecord
{
    private PreventiveValidationRecord()
    {
    }

    private PreventiveValidationRecord(
        Guid id,
        Guid preventiveExecutionId,
        PreventiveValidationActionType actionType,
        Guid validatorUserId,
        DateTimeOffset createdAtUtc,
        string? comment)
    {
        Id = id;
        PreventiveExecutionId = preventiveExecutionId;
        ActionType = actionType;
        ValidatorUserId = validatorUserId;
        CreatedAtUtc = createdAtUtc;
        Comment = NormalizeOptionalComment(comment);
    }

    public Guid Id { get; private set; }

    public Guid PreventiveExecutionId { get; private set; }

    public PreventiveValidationActionType ActionType { get; private set; }

    public Guid ValidatorUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string? Comment { get; private set; }

    public static PreventiveValidationRecord Create(
        Guid preventiveExecutionId,
        PreventiveValidationActionType actionType,
        Guid validatorUserId,
        DateTimeOffset createdAtUtc,
        string? comment)
    {
        if (preventiveExecutionId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive execution id is required for validation history.");
        }

        if (validatorUserId == Guid.Empty)
        {
            throw new DomainRuleException("Preventive validation user is required.");
        }

        return new PreventiveValidationRecord(
            Guid.NewGuid(),
            preventiveExecutionId,
            actionType,
            validatorUserId,
            createdAtUtc,
            comment);
    }

    private static string? NormalizeOptionalComment(string? comment)
    {
        return string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
