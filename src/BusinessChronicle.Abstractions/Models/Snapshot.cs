namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Point-in-time materialized state of an entity at a specific revision.
/// </summary>
public sealed record Snapshot
{
    /// <summary>
    /// Gets the revision this snapshot represents.
    /// </summary>
    public required Identifiers.RevisionId RevisionId { get; init; }

    /// <summary>
    /// Gets the entity the snapshot belongs to.
    /// </summary>
    public required EntityReference Entity { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the snapshot was captured.
    /// </summary>
    public required DateTimeOffset CapturedAt { get; init; }

    /// <summary>
    /// Gets the serialized entity payload.
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; init; }

    /// <summary>
    /// Gets the media type of <see cref="Payload"/> (for example, <c>application/json</c>).
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets optional metadata attached to the snapshot.
    /// </summary>
    public ChronicleMetadata Metadata { get; init; } = ChronicleMetadata.Empty;
}
