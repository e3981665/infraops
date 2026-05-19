using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Domain.Identity.Entities;

public sealed class Permission
{
    private Permission()
    {
    }

    public Permission(Guid id, string code, string description)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Permission id is required.");
        }

        Id = id;
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainRuleException("Permission code is required.");
        }

        return code.Trim().ToLowerInvariant();
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainRuleException("Permission description is required.");
        }

        return description.Trim();
    }
}
