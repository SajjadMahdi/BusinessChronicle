using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

using BusinessChronicle.Storage.InMemory;

namespace BusinessChronicle.Pipeline;

internal sealed class ValidationPipelineStage : ICommitPipelineStage
{
    public string Name => "Validation";

    public ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(state.Context.Message.Text))
        {
            return ValueTask.FromResult(ChronicleResults.Failure(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "Commit message must not be empty.")));
        }

        if (state.Context.StagedRevisions.Count == 0)
        {
            return ValueTask.FromResult(ChronicleResults.Failure(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "At least one revision must be staged.")));
        }

        return ValueTask.FromResult(ChronicleResult.Success);
    }
}

internal sealed class MetadataEnrichmentPipelineStage(IChronicleMetadataProvider metadataProvider) : ICommitPipelineStage
{
    public string Name => "MetadataEnrichment";

    public async ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        ChronicleResult<ChronicleMetadata> enriched = await metadataProvider
            .EnrichAsync(state.Context.Metadata, state.Context, cancellationToken)
            .ConfigureAwait(false);

        if (enriched.IsFailure)
        {
            return ChronicleResults.Failure(enriched.Error!.Value);
        }

        state.EnrichedMetadata = enriched.Value!;
        state.Context.Metadata = enriched.Value!;
        return ChronicleResult.Success;
    }
}

internal sealed class ActorResolutionPipelineStage(IActorResolver actorResolver) : ICommitPipelineStage
{
    public string Name => "ActorResolution";

    public async ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        state.ResolvedActor = state.Context.Author ?? await actorResolver
            .ResolveAsync(cancellationToken)
            .ConfigureAwait(false);

        state.Context.Author = state.ResolvedActor;
        return ChronicleResult.Success;
    }
}

internal sealed class SnapshotPolicyPipelineStage(
    IChronicleStore store,
    Abstractions.Options.ChronicleOptions options) : ICommitPipelineStage
{
    public string Name => "SnapshotPolicy";

    public async ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        if (!options.Entity.CaptureSnapshots)
        {
            return ChronicleResult.Success;
        }

        foreach (RevisionReference reference in state.Context.StagedRevisions)
        {
            ChronicleResult<Snapshot> snapshotResult = await store.RevisionReader
                .ReadSnapshotAsync(reference.Id, cancellationToken)
                .ConfigureAwait(false);

            if (snapshotResult.IsFailure &&
                snapshotResult.Error!.Value.Code == ChronicleErrorCode.NotFound)
            {
                return ChronicleResults.Failure(
                    new ChronicleError(
                        ChronicleErrorCode.SnapshotFailure,
                        $"Snapshot is required for revision '{reference.Id}'."));
            }

            if (snapshotResult.IsFailure)
            {
                return ChronicleResults.Failure(snapshotResult.Error!.Value);
            }
        }

        return ChronicleResult.Success;
    }
}

internal sealed class PersistencePipelineStage(
    IChronicleStore store,
    IChronicleClock clock) : ICommitPipelineStage
{
    public string Name => "Persistence";

    public async ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken)
    {
        CommitContext context = state.InternalContext;
        List<ChronicleEntry> entries = [];

        foreach (RevisionReference reference in context.StagedRevisions)
        {
            ChronicleResult<Revision> readResult = await store.RevisionReader
                .ReadAsync(reference.Id, cancellationToken)
                .ConfigureAwait(false);

            if (readResult.IsFailure)
            {
                return ChronicleResults.Failure(readResult.Error!.Value);
            }

            Revision pending = readResult.Value!;
            Revision active = pending with { State = RevisionState.Active };

            if (store is Storage.InMemory.InMemoryChronicleStore inMemory)
            {
                inMemory.State.ActivateRevision(active);
            }
            else
            {
                ChronicleResult<Revision> writeResult = await store.RevisionWriter
                    .WriteAsync(active, cancellationToken)
                    .ConfigureAwait(false);

                if (writeResult.IsFailure)
                {
                    return ChronicleResults.Failure(writeResult.Error!.Value);
                }
            }

            entries.Add(new ChronicleEntry
            {
                RevisionId = active.Id,
                CommitId = context.CommitId,
                Entity = active.Entity,
                Actor = state.ResolvedActor!,
                OccurredAt = clock.GetUtcNow(),
                Message = context.Message,
                RevisionType = active.Type,
                Changes = store is InMemoryChronicleStore memory
                    ? memory.State.GetRevisionChanges(active.Id)
                    : [],
                Metadata = active.Metadata,
            });
        }

        Commit commit = new()
        {
            Id = context.CommitId,
            Message = context.Message,
            Author = state.ResolvedActor!,
            CommittedAt = clock.GetUtcNow(),
            Revisions = context.StagedRevisions,
            Metadata = state.EnrichedMetadata,
        };

        if (store is Storage.InMemory.InMemoryChronicleStore memoryStore)
        {
            memoryStore.State.AddCommit(commit, entries);
        }

        return ChronicleResult.Success;
    }
}
