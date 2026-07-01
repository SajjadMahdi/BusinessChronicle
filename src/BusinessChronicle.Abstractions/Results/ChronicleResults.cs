namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Factory methods for creating <see cref="ChronicleResult"/> values.
/// </summary>
public static class ChronicleResults
{
    /// <summary>
    /// Creates a successful result with a payload.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="value">The success payload.</param>
    /// <returns>A successful result.</returns>
    public static ChronicleResult<T> Success<T>(T value) => new(value, null);

    /// <summary>
    /// Creates a failed result with a payload type.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="error">The failure descriptor.</param>
    /// <returns>A failed result.</returns>
    public static ChronicleResult<T> Failure<T>(ChronicleError error) => new(default, error);

    /// <summary>
    /// Creates a failed result without a payload.
    /// </summary>
    /// <param name="error">The failure descriptor.</param>
    /// <returns>A failed result.</returns>
    public static ChronicleResult Failure(ChronicleError error) => new(error);
}
