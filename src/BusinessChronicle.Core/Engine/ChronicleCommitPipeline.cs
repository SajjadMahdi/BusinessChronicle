using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;
using BusinessChronicle.Pipeline;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default commit pipeline implementation.
/// </summary>
public sealed class ChronicleCommitPipeline : IChronicleCommitPipeline
{
    private readonly IChronicleMetadataProvider _metadataProvider;
    private readonly IChronicleStore _store;
    private readonly IReadOnlyList<ICommitPipelineStage> _stages;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleCommitPipeline"/> class.
    /// </summary>
    public ChronicleCommitPipeline(
        IActorResolver actorResolver,
        IChronicleMetadataProvider metadataProvider,
        IChronicleStore store,
        IChronicleClock clock,
        ChronicleOptions options,
        IEnumerable<ICommitPipelineStage>? additionalStages = null,
        IEnumerable<ICommitPipelineListener>? listeners = null)
    {
        _metadataProvider = metadataProvider;
        _store = store;

        List<ICommitPipelineStage> stages =
        [
            new ValidationPipelineStage(),
            new MetadataEnrichmentPipelineStage(metadataProvider),
            new ActorResolutionPipelineStage(actorResolver),
            new SnapshotPolicyPipelineStage(store, options),
            new PersistencePipelineStage(store, clock),
            new EventsPipelineStage(store, listeners),
        ];

        if (additionalStages is not null)
        {
            stages.AddRange(additionalStages);
        }

        _stages = stages;
    }

    /// <inheritdoc />
    public async ValueTask<ChronicleResult<ICommitContext>> BeginCommitAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ChronicleMetadata metadata = await _metadataProvider
            .GetMetadataAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        CommitContext context = new(
            new CommitId(IdGenerator.NewId()),
            entity,
            correlation: null)
        {
            Metadata = metadata,
        };

        return ChronicleResults.Success<ICommitContext>(context);
    }

    /// <inheritdoc />
    public async ValueTask<ChronicleResult<Commit>> ExecuteAsync(
        ICommitContext context,
        CancellationToken cancellationToken = default)
    {
        if (context is not CommitContext commitContext)
        {
            return ChronicleResults.Failure<Commit>(
                new ChronicleError(ChronicleErrorCode.ConfigurationError, "Unsupported commit context implementation."));
        }

        CommitPipelineState state = new() { Context = commitContext };

        foreach (ICommitPipelineStage stage in _stages)
        {
            ChronicleResult stageResult = await stage.ExecuteAsync(state, cancellationToken).ConfigureAwait(false);
            if (stageResult.IsFailure)
            {
                return ChronicleResults.Failure<Commit>(stageResult.Error!.Value);
            }
        }

        return await _store.Query.GetCommitAsync(commitContext.CommitId, cancellationToken).ConfigureAwait(false);
    }
}
