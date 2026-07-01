namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Orchestrates the commit pipeline from staged changes to a persisted commit.
/// </summary>
public interface IChronicleCommitPipeline
{
    /// <summary>
    /// Begins a new commit for the specified entity scope.
    /// </summary>
    /// <param name="entity">The primary entity reference for the commit scope.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A commit context or a failure result.</returns>
    ValueTask<Results.ChronicleResult<ICommitContext>> BeginCommitAsync(
        Models.EntityReference entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the commit pipeline and persists the resulting commit.
    /// </summary>
    /// <param name="context">The populated commit context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The persisted commit or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Commit>> ExecuteAsync(
        ICommitContext context,
        CancellationToken cancellationToken = default);
}
