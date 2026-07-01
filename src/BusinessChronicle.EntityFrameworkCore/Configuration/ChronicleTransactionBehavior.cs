namespace BusinessChronicle.EntityFrameworkCore.Configuration;

/// <summary>
/// Transaction participation behavior when chronicle persistence fails.
/// </summary>
public enum ChronicleTransactionBehavior
{
    /// <summary>
    /// Fail the ambient EF transaction when chronicle persistence fails.
    /// </summary>
    FailTransaction = 0,
}

/// <summary>
/// Controls how often snapshots are captured for an entity type.
/// </summary>
public enum SnapshotFrequency
{
    /// <summary>
    /// Capture a snapshot on every revision.
    /// </summary>
    EveryRevision = 0,

    /// <summary>
    /// Capture snapshots only on create and delete revisions.
    /// </summary>
    CreateAndDelete = 1,

    /// <summary>
    /// Never capture snapshots automatically.
    /// </summary>
    Never = 2,
}
