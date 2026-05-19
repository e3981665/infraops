namespace InfraOps.Application.Common.Exceptions;

public class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(string message)
        : base(message)
    {
    }
}
