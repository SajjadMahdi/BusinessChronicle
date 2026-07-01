using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

namespace BusinessChronicle.Storage.InMemory;

/// <summary>
/// In-memory revision reader.
/// </summary>
public sealed class RevisionReader : IRevisionReader
{
    private readonly InMemoryChronicleState _state;

    internal RevisionReader(InMemoryChronicleState state) => _state = state;

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> ReadAsync(
        RevisionId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (id.IsEmpty)
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "Revision identifier must not be empty.")));
        }

        if (!_state.TryGetRevision(id, out Revision revision))
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.NotFound, $"Revision '{id}' was not found.")));
        }

        return ValueTask.FromResult(ChronicleResults.Success(revision));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Snapshot>> ReadSnapshotAsync(
        RevisionId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_state.TryGetSnapshot(id, out Snapshot snapshot))
        {
            return ValueTask.FromResult(ChronicleResults.Failure<Snapshot>(
                new ChronicleError(ChronicleErrorCode.NotFound, $"Snapshot for revision '{id}' was not found.")));
        }

        return ValueTask.FromResult(ChronicleResults.Success(snapshot));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> ListAsync(
        EntityReference entity,
        RevisionListOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<RevisionReference> references = _state.ListRevisionReferences(entity, options);
        return ValueTask.FromResult(ChronicleResults.Success(references));
    }
}

/// <summary>
/// In-memory revision writer.
/// </summary>
public sealed class RevisionWriter : IRevisionWriter
{
    private readonly InMemoryChronicleState _state;

    internal RevisionWriter(InMemoryChronicleState state) => _state = state;

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> WriteAsync(
        Revision revision,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state.AddRevision(revision, activateHead: revision.State == RevisionState.Active);
        return ValueTask.FromResult(ChronicleResults.Success(revision));
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Snapshot>> WriteSnapshotAsync(
        Snapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state.AddSnapshot(snapshot);
        return ValueTask.FromResult(ChronicleResults.Success(snapshot));
    }
}
