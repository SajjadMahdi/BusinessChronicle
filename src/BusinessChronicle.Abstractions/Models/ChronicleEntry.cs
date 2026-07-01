namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Immutable audit record describing a single chronicle event.
/// </summary>
public sealed record ChronicleEntry
{
    /// <summary>
    /// Gets the revision identifier for this entry.
    /// </summary>
    public required Identifiers.RevisionId RevisionId { get; init; }

    /// <summary>
    /// Gets the commit identifier for this entry.
    /// </summary>
    public required Identifiers.CommitId CommitId { get; init; }

    /// <summary>
    /// Gets the affected entity.
    /// </summary>
    public required EntityReference Entity { get; init; }

    /// <summary>
    /// Gets the actor that performed the operation.
    /// </summary>
    public required Actor Actor { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// Gets the commit message associated with the event.
    /// </summary>
    public required CommitMessage Message { get; init; }

    /// <summary>
    /// Gets the revision classification.
    /// </summary>
    public required Enums.RevisionType RevisionType { get; init; }

    /// <summary>
    /// Gets structural changes captured by this entry.
    /// </summary>
    public IReadOnlyList<ChangeDescriptor> Changes { get; init; } = [];

    /// <summary>
    /// Gets metadata attached to the entry.
    /// </summary>
    public ChronicleMetadata Metadata { get; init; } = ChronicleMetadata.Empty;

    /// <summary>
    /// Gets tags attached to the entry.
    /// </summary>
    public IReadOnlyList<ChronicleTag> Tags { get; init; } = [];

    /// <summary>
    /// Gets comments attached to the entry.
    /// </summary>
    public IReadOnlyList<ChronicleComment> Comments { get; init; } = [];
}
