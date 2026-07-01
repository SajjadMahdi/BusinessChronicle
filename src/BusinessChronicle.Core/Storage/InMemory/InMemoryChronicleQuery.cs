using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

namespace BusinessChronicle.Storage.InMemory;

/// <summary>
/// In-memory chronicle query implementation.
/// </summary>
public sealed class ChronicleQuery : IChronicleQuery
{
    private readonly InMemoryChronicleState _state;

    internal ChronicleQuery(InMemoryChronicleState state) => _state = state;

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<TimelineEntry>>> GetTimelineAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<ChronicleEntry> entries = _state.GetEntries(entity, options);
        List<TimelineEntry> timeline = new(entries.Count);
        for (int index = 0; index < entries.Count; index++)
        {
            ChronicleEntry entry = entries[index];
            timeline.Add(new TimelineEntry
            {
                Entry = entry,
                Sequence = index + 1,
                Version = _state.TryGetRevision(entry.RevisionId, out Revision revision) ? revision.Version : null,
            });
        }

        return ValueTask.FromResult(ChronicleResults.Success<IReadOnlyList<TimelineEntry>>(timeline));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<ChronicleEntry>>> GetEntriesAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<ChronicleEntry> entries = _state.GetEntries(entity, options);
        return ValueTask.FromResult(ChronicleResults.Success(entries));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Commit>> GetCommitAsync(
        CommitId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_state.TryGetCommit(id, out Commit commit))
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Commit>(
                new ChronicleError(ChronicleErrorCode.NotFound, $"Commit '{id}' was not found.")));
        }

        return ValueTask.FromResult(ChronicleResults.Success(commit));
    }
}

/// <summary>
/// In-memory linear version graph.
/// </summary>
public sealed class VersionGraph : IVersionGraph
{
    private readonly InMemoryChronicleState _state;

    internal VersionGraph(InMemoryChronicleState state) => _state = state;

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> GetParentAsync(
        RevisionId revisionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_state.TryGetRevision(revisionId, out Revision revision))
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.NotFound, $"Revision '{revisionId}' was not found.")));
        }

        if (revision.ParentRevisionId is not { } parentId)
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.NotFound, "Parent revision was not found.")));
        }

        if (!_state.TryGetRevision(parentId, out Revision parent))
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.NotFound, "Parent revision was not found.")));
        }

        return ValueTask.FromResult(ChronicleResults.Success(parent));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<Revision>>> GetChildrenAsync(
        RevisionId revisionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Revision> children = _state.GetChildren(revisionId);
        return ValueTask.FromResult(ChronicleResults.Success(children));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> GetAncestorsAsync(
        RevisionId revisionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<RevisionReference> ancestors = _state.GetAncestors(revisionId);
        return ValueTask.FromResult(ChronicleResults.Success(ancestors));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> GetHeadAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Revision? head = _state.GetHeadRevision(entity);
        if (head is null)
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.NotFound, "Head revision was not found.")));
        }

        return ValueTask.FromResult(ChronicleResults.Success(head));
    }
}
