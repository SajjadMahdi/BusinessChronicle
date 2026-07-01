namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Serializes and deserializes chronicle payloads such as snapshots.
/// </summary>
/// <remarks>
/// Trimming and Native AOT hosts should register source-generated serializers in a future hosting package.
/// </remarks>
public interface IChronicleSerializer
{
    /// <summary>
    /// Serializes a value to a UTF-8 encoded payload.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The serialized payload or a failure result.</returns>
    ValueTask<Results.ChronicleResult<ReadOnlyMemory<byte>>> SerializeAsync<T>(
        T value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes a UTF-8 encoded payload to a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="payload">The payload to deserialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized value or a failure result.</returns>
    ValueTask<Results.ChronicleResult<T>> DeserializeAsync<T>(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default);
}
