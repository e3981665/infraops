namespace InfraOps.Application.Common.Exceptions;

public class ApplicationUnauthorizedException : Exception
{
    public ApplicationUnauthorizedException(string message)
        : base(message)
    {
    }
}
