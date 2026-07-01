namespace BusinessChronicle.Abstractions.Exceptions;

/// <summary>
/// Base exception for unexpected chronicle failures.
/// Expected failures should be represented by <see cref="Results.ChronicleResult"/> instead.
/// </summary>
public class ChronicleException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleException"/> class.
    /// </summary>
    public ChronicleException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ChronicleException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ChronicleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
