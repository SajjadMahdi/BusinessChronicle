using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Serialization;

namespace BusinessChronicle.Engine;

/// <summary>
/// System.Text.Json serializer with source-generation defaults.
/// </summary>
public sealed class DefaultChronicleSerializer : IChronicleSerializer
{
    private readonly ChronicleJsonSerializerContext _context;

    /// <summary>
    /// Initializes a new instance using source-generated defaults.
    /// </summary>
    public DefaultChronicleSerializer()
        : this(ChronicleJsonSerializerContext.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance with a source-generated serializer context.
    /// </summary>
    /// <param name="context">The serializer context.</param>
    public DefaultChronicleSerializer(ChronicleJsonSerializerContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<ReadOnlyMemory<byte>>> SerializeAsync<T>(
        T value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            JsonTypeInfo? typeInfo = _context.GetTypeInfo(typeof(T));
            if (typeInfo is null)
            {
                return ValueTask.FromResult(ChronicleResults.Failure<ReadOnlyMemory<byte>>(
                    new ChronicleError(
                        ChronicleErrorCode.NotSupported,
                        $"Type '{typeof(T).FullName}' is not registered in {nameof(ChronicleJsonSerializerContext)}.")));
            }

            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);
            return ValueTask.FromResult(ChronicleResults.Success<ReadOnlyMemory<byte>>(buffer));
        }
        catch (JsonException ex)
        {
            return ValueTask.FromResult(ChronicleResults.Failure<ReadOnlyMemory<byte>>(
                new ChronicleError(ChronicleErrorCode.StorageFailure, "Serialization failed.", ex.Message)));
        }
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<T>> DeserializeAsync<T>(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            JsonTypeInfo? typeInfo = _context.GetTypeInfo(typeof(T));
            if (typeInfo is null)
            {
                return ValueTask.FromResult(ChronicleResults.Failure<T>(
                    new ChronicleError(
                        ChronicleErrorCode.NotSupported,
                        $"Type '{typeof(T).FullName}' is not registered in {nameof(ChronicleJsonSerializerContext)}.")));
            }

            object? value = JsonSerializer.Deserialize(payload.Span, typeInfo);
            if (value is not T typed)
            {
                return ValueTask.FromResult(ChronicleResults.Failure<T>(
                    new ChronicleError(ChronicleErrorCode.StorageFailure, "Deserialization returned null.")));
            }

            return ValueTask.FromResult(ChronicleResults.Success(typed));
        }
        catch (JsonException ex)
        {
            return ValueTask.FromResult(ChronicleResults.Failure<T>(
                new ChronicleError(ChronicleErrorCode.StorageFailure, "Deserialization failed.", ex.Message)));
        }
    }
}
