namespace BusinessChronicle.Abstractions.Enums;

/// <summary>
/// Lifecycle state of a revision within the version graph.
/// </summary>
public enum RevisionState
{
    /// <summary>
    /// The revision is the current head for its entity lineage.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The revision was replaced by a newer revision but remains addressable.
    /// </summary>
    Superseded = 1,

    /// <summary>
    /// The revision is retained for audit but excluded from active queries.
    /// </summary>
    Archived = 2,

    /// <summary>
    /// The revision is staged and not yet committed.
    /// </summary>
    Pending = 3,
}
