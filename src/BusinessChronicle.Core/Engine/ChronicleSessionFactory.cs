using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;

namespace BusinessChronicle.Engine;

/// <summary>
/// Creates scoped chronicle sessions.
/// </summary>
public sealed class ChronicleSessionFactory(
    IChronicleCommitPipeline pipeline,
    IChronicleStore store,
    IRevisionFactory revisionFactory,
    IRevisionComparer revisionComparer,
    IChronicleClock clock,
    IChronicleSerializer serializer,
    ChronicleOptions options)
{
    /// <summary>
    /// Creates a session for the specified entity scope.
    /// </summary>
    public async ValueTask<ChronicleResult<IChronicleSession>> CreateAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        ChronicleResult<ICommitContext> beginResult = await pipeline
            .BeginCommitAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (beginResult.IsFailure)
        {
            return ChronicleResults.Failure<IChronicleSession>(beginResult.Error!.Value);
        }

        IChronicleSession session = new ChronicleSession(
            pipeline,
            store,
            revisionFactory,
            revisionComparer,
            clock,
            serializer,
            options,
            beginResult.Value!);

        return ChronicleResults.Success(session);
    }
}
