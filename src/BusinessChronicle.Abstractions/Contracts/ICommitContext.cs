namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Mutable context accumulated while constructing a commit.
/// </summary>
public interface ICommitContext
{
    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    Models.CommitMessage Message { get; set; }

    /// <summary>
    /// Gets or sets the author of the commit.
    /// </summary>
    Models.Actor? Author { get; set; }

    /// <summary>
    /// Gets or sets commit-level metadata.
    /// </summary>
    Models.ChronicleMetadata Metadata { get; set; }

    /// <summary>
    /// Gets the entity revisions staged for this commit.
    /// </summary>
    IReadOnlyList<Models.RevisionReference> StagedRevisions { get; }

    /// <summary>
    /// Gets correlation identifiers propagated to storage and telemetry.
    /// </summary>
    FrozenDictionary<string, string> Correlation { get; }

    /// <summary>
    /// Stages a revision reference to be included in the commit.
    /// </summary>
    /// <param name="revision">The revision reference to stage.</param>
    void StageRevision(Models.RevisionReference revision);
}
