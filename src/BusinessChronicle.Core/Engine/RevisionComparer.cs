using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default revision comparer built on the diff engine.
/// </summary>
public sealed class RevisionComparer(
    IChronicleDiffEngine diffEngine,
    IChronicleStore store) : IRevisionComparer
{
    /// <inheritdoc />
    public async ValueTask<ChronicleResult<RevisionComparisonResult>> CompareAsync(
        ComparisonTarget target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ChronicleResult<IReadOnlyList<ChangeDescriptor>> diffResult = await diffEngine
            .DiffAsync(target, cancellationToken)
            .ConfigureAwait(false);

        if (diffResult.IsFailure)
        {
            return ChronicleResults.Failure<RevisionComparisonResult>(diffResult.Error!.Value);
        }

        Snapshot? sourceSnapshot = null;
        Snapshot? targetSnapshot = null;

        if (target.SourceRevisionId is { } sourceRevisionId)
        {
            ChronicleResult<Snapshot> sourceSnapshotResult = await store.RevisionReader
                .ReadSnapshotAsync(sourceRevisionId, cancellationToken)
                .ConfigureAwait(false);

            if (sourceSnapshotResult.IsSuccess)
            {
                sourceSnapshot = sourceSnapshotResult.Value;
            }
        }

        if (target.TargetRevisionId is { } targetRevisionId)
        {
            ChronicleResult<Snapshot> targetSnapshotResult = await store.RevisionReader
                .ReadSnapshotAsync(targetRevisionId, cancellationToken)
                .ConfigureAwait(false);

            if (targetSnapshotResult.IsSuccess)
            {
                targetSnapshot = targetSnapshotResult.Value;
            }
        }

        return ChronicleResults.Success(new RevisionComparisonResult
        {
            Target = target,
            Changes = diffResult.Value!,
            SourceSnapshot = sourceSnapshot,
            TargetSnapshot = targetSnapshot,
        });
    }
}
