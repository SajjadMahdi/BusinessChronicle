namespace BusinessChronicle.Abstractions.Exceptions;

/// <summary>
/// Thrown when a requested revision does not exist or is not visible in the current scope.
/// </summary>
public class RevisionNotFoundException : ChronicleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionNotFoundException"/> class.
    /// </summary>
    public RevisionNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public RevisionNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RevisionNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
