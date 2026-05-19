namespace InfraOps.Domain.Common.Exceptions;

public sealed class DomainRuleException : Exception
{
    public DomainRuleException(string message)
        : base(message)
    {
    }
}
