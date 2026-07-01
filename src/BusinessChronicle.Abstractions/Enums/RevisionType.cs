namespace BusinessChronicle.Abstractions.Enums;

/// <summary>
/// Describes the kind of change represented by a revision.
/// </summary>
public enum RevisionType
{
    /// <summary>
    /// Entity was created.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Entity was updated.
    /// </summary>
    Update = 1,

    /// <summary>
    /// Entity was deleted or marked deleted.
    /// </summary>
    Delete = 2,

    /// <summary>
    /// Entity was restored from a prior state.
    /// </summary>
    Restore = 3,

    /// <summary>
    /// Entity was reverted via rollback.
    /// </summary>
    Rollback = 4,

    /// <summary>
    /// A point-in-time snapshot was captured without a semantic mutation.
    /// </summary>
    Snapshot = 5,
}
