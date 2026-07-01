namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Immutable revision of an entity within the chronicle version graph.
/// </summary>
public sealed record Revision
{
    /// <summary>
    /// Gets the revision identifier.
    /// </summary>
    public required Identifiers.RevisionId Id { get; init; }

    /// <summary>
    /// Gets the commit that owns this revision.
    /// </summary>
    public required Identifiers.CommitId CommitId { get; init; }

    /// <summary>
    /// Gets the entity this revision applies to.
    /// </summary>
    public required EntityReference Entity { get; init; }

    /// <summary>
    /// Gets the revision classification.
    /// </summary>
    public required Enums.RevisionType Type { get; init; }

    /// <summary>
    /// Gets the lifecycle state of the revision.
    /// </summary>
    public required Enums.RevisionState State { get; init; }

    /// <summary>
    /// Gets the parent revision identifier when this revision extends a lineage.
    /// </summary>
    public Identifiers.RevisionId? ParentRevisionId { get; init; }

    /// <summary>
    /// Gets the version pointer assigned to this revision.
    /// </summary>
    public VersionPointer? Version { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the revision was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets optional metadata attached to the revision.
    /// </summary>
    public ChronicleMetadata Metadata { get; init; } = ChronicleMetadata.Empty;
}
