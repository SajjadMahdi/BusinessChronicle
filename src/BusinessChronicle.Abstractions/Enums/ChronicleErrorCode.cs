namespace BusinessChronicle.Abstractions.Enums;

/// <summary>
/// Stable error codes for expected chronicle operation failures.
/// </summary>
public enum ChronicleErrorCode
{
    /// <summary>
    /// No error; operation succeeded.
    /// </summary>
    None = 0,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// Input or domain validation failed.
    /// </summary>
    ValidationFailed = 2,

    /// <summary>
    /// A concurrency conflict prevented the operation.
    /// </summary>
    ConcurrencyConflict = 3,

    /// <summary>
    /// The underlying storage provider failed.
    /// </summary>
    StorageFailure = 4,

    /// <summary>
    /// Chronicle configuration is invalid or incomplete.
    /// </summary>
    ConfigurationError = 5,

    /// <summary>
    /// The operation is not supported in the current context.
    /// </summary>
    NotSupported = 6,

    /// <summary>
    /// The operation was cancelled.
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Snapshot materialization or retrieval failed.
    /// </summary>
    SnapshotFailure = 8,

    /// <summary>
    /// Rollback preconditions were not satisfied.
    /// </summary>
    RollbackFailure = 9,
}
