namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Represents the outcome of a chronicle operation that may fail without throwing.
/// </summary>
/// <typeparam name="T">The success payload type.</typeparam>
public readonly record struct ChronicleResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleResult{T}"/> struct.
    /// </summary>
    /// <param name="value">The success value when the operation succeeded.</param>
    /// <param name="error">The error when the operation failed.</param>
    public ChronicleResult(T? value, ChronicleError? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Gets the success value when <see cref="IsSuccess"/> is <see langword="true"/>.
    /// </summary>
    public T? Value { get; }

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
}
