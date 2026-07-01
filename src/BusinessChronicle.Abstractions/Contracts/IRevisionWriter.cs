namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Persists revisions and snapshots to storage.
/// </summary>
public interface IRevisionWriter
{
    /// <summary>
    /// Persists a revision.
    /// </summary>
    /// <param name="revision">The revision to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The persisted revision or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Revision>> WriteAsync(
        Models.Revision revision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The persisted snapshot or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Snapshot>> WriteSnapshotAsync(
        Models.Snapshot snapshot,
        CancellationToken cancellationToken = default);
}
