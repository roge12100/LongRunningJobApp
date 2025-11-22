namespace LongRunningJobApp.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid job state transition is attempted
/// </summary>
public class InvalidJobStateTransitionException : Exception
{
    public InvalidJobStateTransitionException()
    {
    }

    public InvalidJobStateTransitionException(string message) 
        : base(message)
    {
    }

    public InvalidJobStateTransitionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
