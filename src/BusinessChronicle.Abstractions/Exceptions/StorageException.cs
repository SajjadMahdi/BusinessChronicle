namespace BusinessChronicle.Abstractions.Exceptions;

/// <summary>
/// Thrown when the underlying storage provider fails unexpectedly.
/// </summary>
public class StorageException : ChronicleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    public StorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public StorageException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
