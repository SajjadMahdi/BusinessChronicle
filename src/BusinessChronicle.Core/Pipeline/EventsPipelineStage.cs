using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

namespace BusinessChronicle.Pipeline;

/// <summary>
/// Receives commit pipeline events.
/// </summary>
public interface ICommitPipelineListener
{
    /// <summary>
    /// Invoked after a commit has been persisted.
    /// </summary>
    ValueTask OnCommitPersistedAsync(Commit commit, CancellationToken cancellationToken = default);
}

internal sealed class EventsPipelineStage(
    IChronicleStore store,
    IEnumerable<ICommitPipelineListener>? listeners = null) : ICommitPipelineStage
{
    private readonly ICommitPipelineListener[] _listeners = listeners?.ToArray() ?? [];

    public string Name => "Events";

    public async ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        if (_listeners.Length == 0)
        {
            return ChronicleResult.Success;
        }

        ChronicleResult<Commit> commitResult = await store.Query
            .GetCommitAsync(state.InternalContext.CommitId, cancellationToken)
            .ConfigureAwait(false);

        if (commitResult.IsFailure)
        {
            return ChronicleResults.Failure(commitResult.Error!.Value);
        }

        foreach (ICommitPipelineListener listener in _listeners)
        {
            await listener.OnCommitPersistedAsync(commitResult.Value!, cancellationToken).ConfigureAwait(false);
        }

        return ChronicleResult.Success;
    }
}
