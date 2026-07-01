using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Engine;

namespace BusinessChronicle.AspNetCore.Services;

/// <summary>
/// HTTP-facing chronicle operations.
/// </summary>
public interface IChronicleApiService
{
    ValueTask<ChronicleResult<IReadOnlyList<ChronicleEntry>>> GetHistoryAsync(EntityReference entity, TimelineQueryOptions options, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<IReadOnlyList<TimelineEntry>>> GetTimelineAsync(EntityReference entity, TimelineQueryOptions options, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<Revision>> GetRevisionAsync(RevisionId revisionId, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<RevisionComparisonResult>> CompareAsync(ComparisonTarget target, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<Revision>> RollbackAsync(EntityReference entity, RevisionId targetRevisionId, CommitMessage message, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<Revision>> GetHeadAsync(EntityReference entity, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> ListRevisionsAsync(EntityReference entity, RevisionListOptions options, CancellationToken cancellationToken);

    ValueTask<ChronicleResult<Commit>> GetCommitAsync(CommitId commitId, CancellationToken cancellationToken);
}

/// <summary>
/// Default HTTP-facing chronicle operations.
/// </summary>
public sealed class ChronicleApiService(
    IChronicleStore store,
    ChronicleSessionFactory sessionFactory) : IChronicleApiService
{
    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<ChronicleEntry>>> GetHistoryAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken) =>
        store.Query.GetEntriesAsync(entity, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<TimelineEntry>>> GetTimelineAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken) =>
        store.Query.GetTimelineAsync(entity, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> GetRevisionAsync(
        RevisionId revisionId,
        CancellationToken cancellationToken) =>
        store.RevisionReader.ReadAsync(revisionId, cancellationToken);

    /// <inheritdoc />
    public async ValueTask<ChronicleResult<RevisionComparisonResult>> CompareAsync(
        ComparisonTarget target,
        CancellationToken cancellationToken)
    {
        ChronicleResult<IChronicleSession> sessionResult = await sessionFactory
            .CreateAsync(target.Entity, cancellationToken)
            .ConfigureAwait(false);

        if (sessionResult.IsFailure)
        {
            return ChronicleResults.Failure<RevisionComparisonResult>(sessionResult.Error!.Value);
        }

        await using IChronicleSession session = sessionResult.Value!;
        if (session is not ChronicleSession concrete)
        {
            return ChronicleResults.Failure<RevisionComparisonResult>(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "Chronicle session implementation is not supported."));
        }

        return await concrete.CompareAsync(target, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<ChronicleResult<Revision>> RollbackAsync(
        EntityReference entity,
        RevisionId targetRevisionId,
        CommitMessage message,
        CancellationToken cancellationToken)
    {
        ChronicleResult<IChronicleSession> sessionResult = await sessionFactory
            .CreateAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (sessionResult.IsFailure)
        {
            return ChronicleResults.Failure<Revision>(sessionResult.Error!.Value);
        }

        await using IChronicleSession session = sessionResult.Value!;
        if (session is not ChronicleSession concrete)
        {
            return ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "Chronicle session implementation is not supported."));
        }

        ChronicleResult<Revision> rollbackResult = await concrete
            .RollbackAsync(entity, targetRevisionId, message, cancellationToken)
            .ConfigureAwait(false);

        if (rollbackResult.IsFailure)
        {
            return rollbackResult;
        }

        ChronicleResult<Commit> commitResult = await session.CommitAsync(cancellationToken).ConfigureAwait(false);
        return commitResult.IsFailure
            ? ChronicleResults.Failure<Revision>(commitResult.Error!.Value)
            : rollbackResult;
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Revision>> GetHeadAsync(
        EntityReference entity,
        CancellationToken cancellationToken) =>
        store.VersionGraph.GetHeadAsync(entity, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> ListRevisionsAsync(
        EntityReference entity,
        RevisionListOptions options,
        CancellationToken cancellationToken) =>
        store.RevisionReader.ListAsync(entity, options, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ChronicleResult<Commit>> GetCommitAsync(
        CommitId commitId,
        CancellationToken cancellationToken) =>
        store.Query.GetCommitAsync(commitId, cancellationToken);
}
