using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

using BusinessChronicle.Storage.InMemory;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default chronicle session implementation.
/// </summary>
public sealed class ChronicleSession : IChronicleSession
{
    private readonly IChronicleCommitPipeline _pipeline;
    private readonly IChronicleStore _store;
    private readonly IRevisionFactory _revisionFactory;
    private readonly IRevisionComparer _revisionComparer;
    private readonly IChronicleClock _clock;
    private readonly ChronicleOptions _options;
    private readonly IChronicleSerializer _serializer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleSession"/> class.
    /// </summary>
    public ChronicleSession(
        IChronicleCommitPipeline pipeline,
        IChronicleStore store,
        IRevisionFactory revisionFactory,
        IRevisionComparer revisionComparer,
        IChronicleClock clock,
        IChronicleSerializer serializer,
        ChronicleOptions options,
        ICommitContext commitContext)
    {
        _pipeline = pipeline;
        _store = store;
        _revisionFactory = revisionFactory;
        _revisionComparer = revisionComparer;
        _clock = clock;
        _serializer = serializer;
        _options = options;

        if (commitContext is not CommitContext context)
        {
            throw new ArgumentException("Commit context implementation is not supported.", nameof(commitContext));
        }

        CommitContext = context;
    }

    /// <inheritdoc />
    public ICommitContext CommitContext { get; }

    /// <inheritdoc />
    public IRevisionFactory RevisionFactory => _revisionFactory;

    private CommitContext InternalCommitContext => (CommitContext)CommitContext;

    /// <summary>
    /// Begins a new commit for the specified entity scope.
    /// </summary>
    public async ValueTask<ChronicleResult<ICommitContext>> CreateCommitAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _pipeline.BeginCommitAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves a revision and optionally its snapshot, staging it in the current commit.
    /// </summary>
    public async ValueTask<ChronicleResult<Revision>> SaveRevisionAsync(
        EntityReference entity,
        RevisionType revisionType,
        IReadOnlyList<ChangeDescriptor> changes,
        ReadOnlyMemory<byte>? snapshotPayload = null,
        string? snapshotContentType = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        CommitContext commitContext = InternalCommitContext;

        RevisionId? parentRevisionId = null;
        ChronicleResult<Revision> headResult = await _store.VersionGraph
            .GetHeadAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (headResult.IsSuccess)
        {
            parentRevisionId = headResult.Value!.Id;
        }

        RevisionContext revisionContext = new(
            entity,
            revisionType,
            commitContext.CommitId,
            parentRevisionId,
            changes,
            commitContext.Metadata,
            version: null);

        ChronicleResult<Revision> createResult = await _revisionFactory
            .CreateAsync(revisionContext, cancellationToken)
            .ConfigureAwait(false);

        if (createResult.IsFailure)
        {
            return createResult;
        }

        Revision revision = createResult.Value!;
        ChronicleResult<Revision> writeResult = await _store.RevisionWriter
            .WriteAsync(revision, cancellationToken)
            .ConfigureAwait(false);

        if (writeResult.IsFailure)
        {
            return writeResult;
        }

        if (_options.Entity.CaptureSnapshots)
        {
            ChronicleResult<Snapshot> snapshotResult = await WriteSnapshotAsync(
                revision,
                snapshotPayload,
                snapshotContentType,
                cancellationToken).ConfigureAwait(false);

            if (snapshotResult.IsFailure)
            {
                return ChronicleResults.Failure<Revision>(snapshotResult.Error!.Value);
            }
        }

        commitContext.StageRevision(new RevisionReference(revision.Id, revision.Entity));

        if (_store is InMemoryChronicleStore inMemoryStore)
        {
            inMemoryStore.State.SetRevisionChanges(revision.Id, changes);
        }

        return ChronicleResults.Success(revision);
    }

    /// <inheritdoc />
    public async ValueTask<ChronicleResult<Commit>> CommitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _pipeline.ExecuteAsync(CommitContext, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets chronicle history entries for an entity.
    /// </summary>
    public ValueTask<ChronicleResult<IReadOnlyList<ChronicleEntry>>> GetHistoryAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _store.Query.GetEntriesAsync(entity, options, cancellationToken);
    }

    /// <summary>
    /// Compares two revisions or versions for an entity.
    /// </summary>
    public ValueTask<ChronicleResult<RevisionComparisonResult>> CompareAsync(
        ComparisonTarget target,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _revisionComparer.CompareAsync(target, cancellationToken);
    }

    /// <summary>
    /// Rolls back an entity to a prior revision.
    /// </summary>
    public async ValueTask<ChronicleResult<Revision>> RollbackAsync(
        EntityReference entity,
        RevisionId targetRevisionId,
        CommitMessage message,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Entity.AllowRollback)
        {
            return ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.RollbackFailure, "Rollback is disabled for this entity."));
        }

        ChronicleResult<Snapshot> snapshotResult = await _store.RevisionReader
            .ReadSnapshotAsync(targetRevisionId, cancellationToken)
            .ConfigureAwait(false);

        ReadOnlyMemory<byte>? payload = snapshotResult.IsSuccess ? snapshotResult.Value!.Payload : null;
        InternalCommitContext.Message = message;

        return await SaveRevisionAsync(
            entity,
            RevisionType.Rollback,
            [],
            payload,
            snapshotResult.Value?.ContentType,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets timeline entries for an entity.
    /// </summary>
    public ValueTask<ChronicleResult<IReadOnlyList<TimelineEntry>>> TimelineAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _store.Query.GetTimelineAsync(entity, options, cancellationToken);
    }

    /// <summary>
    /// Gets the latest revision for an entity.
    /// </summary>
    public ValueTask<ChronicleResult<Revision>> GetLatestRevisionAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _store.VersionGraph.GetHeadAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Gets a revision by identifier.
    /// </summary>
    public ValueTask<ChronicleResult<Revision>> GetRevisionAsync(
        RevisionId revisionId,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _store.RevisionReader.ReadAsync(revisionId, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private async ValueTask<ChronicleResult<Snapshot>> WriteSnapshotAsync(
        Revision revision,
        ReadOnlyMemory<byte>? payload,
        string? contentType,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> snapshotPayload = payload ?? ReadOnlyMemory<byte>.Empty;
        if (payload is null)
        {
            ChronicleResult<ReadOnlyMemory<byte>> serialized = await _serializer
                .SerializeAsync(revision.Metadata.Values.ToDictionary(static pair => pair.Key, static pair => pair.Value), cancellationToken)
                .ConfigureAwait(false);

            if (serialized.IsFailure)
            {
                return ChronicleResults.Failure<Snapshot>(serialized.Error!.Value);
            }

            snapshotPayload = serialized.Value!;
        }

        if (snapshotPayload.Length > _options.Snapshot.MaxPayloadBytes)
        {
            return ChronicleResults.Failure<Snapshot>(
                new ChronicleError(ChronicleErrorCode.SnapshotFailure, "Snapshot payload exceeds configured limits."));
        }

        Snapshot snapshot = new()
        {
            RevisionId = revision.Id,
            Entity = revision.Entity,
            CapturedAt = _clock.GetUtcNow(),
            Payload = snapshotPayload,
            ContentType = contentType ?? _options.Snapshot.DefaultContentType,
            Metadata = revision.Metadata,
        };

        return await _store.RevisionWriter.WriteSnapshotAsync(snapshot, cancellationToken).ConfigureAwait(false);
    }
}
