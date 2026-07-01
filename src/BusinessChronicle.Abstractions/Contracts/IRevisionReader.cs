namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Reads revisions and snapshots from storage.
/// </summary>
public interface IRevisionReader
{
    /// <summary>
    /// Reads a revision by identifier.
    /// </summary>
    /// <param name="id">The revision identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The revision or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Revision>> ReadAsync(
        Identifiers.RevisionId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the snapshot associated with a revision.
    /// </summary>
    /// <param name="id">The revision identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The snapshot or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Snapshot>> ReadSnapshotAsync(
        Identifiers.RevisionId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists revision references for an entity.
    /// </summary>
    /// <param name="entity">The entity to query.</param>
    /// <param name="options">Query options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Matching revision references or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.RevisionReference>>> ListAsync(
        Models.EntityReference entity,
        Options.RevisionListOptions options,
        CancellationToken cancellationToken = default);
}
