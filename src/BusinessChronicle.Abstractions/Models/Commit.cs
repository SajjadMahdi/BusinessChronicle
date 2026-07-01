namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// A commit groups one or more revisions under a single author, message, and timestamp.
/// </summary>
public sealed record Commit
{
    /// <summary>
    /// Gets the commit identifier.
    /// </summary>
    public required Identifiers.CommitId Id { get; init; }

    /// <summary>
    /// Gets the commit message.
    /// </summary>
    public required CommitMessage Message { get; init; }

    /// <summary>
    /// Gets the author of the commit.
    /// </summary>
    public required Actor Author { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the commit was recorded.
    /// </summary>
    public required DateTimeOffset CommittedAt { get; init; }

    /// <summary>
    /// Gets the parent commit identifier when this commit extends a branch.
    /// </summary>
    public Identifiers.CommitId? ParentCommitId { get; init; }

    /// <summary>
    /// Gets lightweight references to revisions contained in this commit.
    /// </summary>
    public IReadOnlyList<RevisionReference> Revisions { get; init; } = [];

    /// <summary>
    /// Gets optional metadata attached to the commit.
    /// </summary>
    public ChronicleMetadata Metadata { get; init; } = ChronicleMetadata.Empty;
}
