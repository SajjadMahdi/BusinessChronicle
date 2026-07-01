namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Represents the outcome of a chronicle operation that returns no payload on success.
/// </summary>
public readonly record struct ChronicleResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleResult"/> struct.
    /// </summary>
    /// <param name="error">The error when the operation failed; otherwise <see langword="null"/>.</param>
    public ChronicleResult(ChronicleError? error)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the error when <see cref="IsSuccess"/> is <see langword="false"/>.
    /// </summary>
    public ChronicleError? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets a singleton successful result.
    /// </summary>
    public static ChronicleResult Success { get; } = new(null);
}
