namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Queries chronicle history projections such as timelines and audit entries.
/// </summary>
public interface IChronicleQuery
{
    /// <summary>
    /// Gets timeline entries for an entity.
    /// </summary>
    /// <param name="entity">The entity to query.</param>
    /// <param name="options">Timeline query options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Timeline entries or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.TimelineEntry>>> GetTimelineAsync(
        Models.EntityReference entity,
        Options.TimelineQueryOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chronicle entries for an entity.
    /// </summary>
    /// <param name="entity">The entity to query.</param>
    /// <param name="options">Timeline query options reused for chronological filtering.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Chronicle entries or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.ChronicleEntry>>> GetEntriesAsync(
        Models.EntityReference entity,
        Options.TimelineQueryOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a commit by identifier.
    /// </summary>
    /// <param name="id">The commit identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The commit or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Commit>> GetCommitAsync(
        Identifiers.CommitId id,
        CancellationToken cancellationToken = default);
}
