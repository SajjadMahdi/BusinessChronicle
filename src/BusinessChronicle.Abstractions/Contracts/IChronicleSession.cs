namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Represents a scoped unit of work for recording chronicle changes.
/// </summary>
public interface IChronicleSession : IAsyncDisposable
{
    /// <summary>
    /// Gets the commit context for the current session.
    /// </summary>
    ICommitContext CommitContext { get; }

    /// <summary>
    /// Gets the revision context factory for the current session.
    /// </summary>
    IRevisionFactory RevisionFactory { get; }

    /// <summary>
    /// Commits staged changes and persists them through the configured pipeline.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The persisted commit or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Commit>> CommitAsync(
        CancellationToken cancellationToken = default);
}
