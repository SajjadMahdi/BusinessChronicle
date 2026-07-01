namespace BusinessChronicle.Abstractions.Exceptions;

/// <summary>
/// Thrown when a rollback operation fails unexpectedly.
/// </summary>
public class RollbackException : ChronicleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackException"/> class.
    /// </summary>
    public RollbackException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public RollbackException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RollbackException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
