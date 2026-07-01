using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;
using BusinessChronicle.Storage.InMemory;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Engine;

/// <summary>
/// Creates validated revisions from revision contexts.
/// </summary>
public sealed class RevisionFactory(
    IChronicleClock clock,
    IChronicleStore store) : IRevisionFactory
{
    /// <inheritdoc />
    public async ValueTask<ChronicleResult<Revision>> CreateAsync(
        IRevisionContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        long versionNumber = await ResolveVersionNumberAsync(context, cancellationToken).ConfigureAwait(false);
        VersionPointer version = context.Version ?? new VersionPointer { Number = versionNumber };

        Revision revision = new()
        {
            Id = new RevisionId(IdGenerator.NewId()),
            CommitId = context.CommitId,
            Entity = context.Entity,
            Type = context.Type,
            State = RevisionState.Pending,
            ParentRevisionId = context.ParentRevisionId,
            Version = version,
            CreatedAt = clock.GetUtcNow(),
            Metadata = context.Metadata,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateRevision(revision);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(revision);
    }

    private async ValueTask<long> ResolveVersionNumberAsync(
        IRevisionContext context,
        CancellationToken cancellationToken)
    {
        if (store is InMemoryChronicleStore inMemoryStore)
        {
            return inMemoryStore.State.GetNextVersion(context.Entity);
        }

        ChronicleResult<Revision> headResult =
            await store.VersionGraph.GetHeadAsync(context.Entity, cancellationToken).ConfigureAwait(false);

        if (headResult.IsSuccess && headResult.Value!.Version is { } headVersion)
        {
            return headVersion.Number + 1;
        }

        return 1;
    }
}
