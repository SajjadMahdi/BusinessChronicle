namespace BusinessChronicle.Abstractions.Exceptions;

/// <summary>
/// Thrown when snapshot capture or retrieval fails unexpectedly.
/// </summary>
public class SnapshotException : ChronicleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotException"/> class.
    /// </summary>
    public SnapshotException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public SnapshotException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SnapshotException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
