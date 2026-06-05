namespace Planova.Application.Exceptions;

public class InvalidTransitionException : ApplicationException
{
    public InvalidTransitionException(string fromStatus, string toStatus)
        : base($"Cannot transition from '{fromStatus}' to '{toStatus}'.")
    {
    }
}
