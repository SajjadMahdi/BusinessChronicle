using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="Snapshot"/> instances.
/// </summary>
public sealed class SnapshotBuilder
{
    private RevisionId _revisionId;
    private EntityReference _entity = null!;
    private DateTimeOffset _capturedAt;
    private ReadOnlyMemory<byte> _payload;
    private string? _contentType;
    private ChronicleMetadata _metadata = ChronicleMetadata.Empty;

    /// <summary>
    /// Sets the revision identifier.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithRevisionId(RevisionId revisionId)
    {
        _revisionId = revisionId;
        return this;
    }

    /// <summary>
    /// Sets the entity reference.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithEntity(EntityReference entity)
    {
        _entity = entity;
        return this;
    }

    /// <summary>
    /// Sets the capture timestamp.
    /// </summary>
    /// <param name="capturedAt">The UTC capture timestamp.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithCapturedAt(DateTimeOffset capturedAt)
    {
        _capturedAt = capturedAt;
        return this;
    }

    /// <summary>
    /// Sets the serialized payload.
    /// </summary>
    /// <param name="payload">The payload bytes.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithPayload(ReadOnlyMemory<byte> payload)
    {
        _payload = payload;
        return this;
    }

    /// <summary>
    /// Sets the payload content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithContentType(string? contentType)
    {
        _contentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets snapshot metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The builder instance.</returns>
    public SnapshotBuilder WithMetadata(ChronicleMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Builds a validated snapshot.
    /// </summary>
    /// <returns>A result containing the snapshot or validation failure.</returns>
    public ChronicleResult<Snapshot> Build()
    {
        Snapshot snapshot = new()
        {
            RevisionId = _revisionId,
            Entity = _entity,
            CapturedAt = _capturedAt,
            Payload = _payload,
            ContentType = _contentType,
            Metadata = _metadata,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateSnapshot(snapshot);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<Snapshot>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(snapshot);
    }
}
