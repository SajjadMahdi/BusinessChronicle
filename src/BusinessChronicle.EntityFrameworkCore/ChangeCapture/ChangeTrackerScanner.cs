using System.Buffers;
using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Metadata;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BusinessChronicle.EntityFrameworkCore.ChangeCapture;

/// <summary>
/// Captured entity change pending persistence.
/// </summary>
internal sealed class CapturedEntityChange
{
    internal required EntityDescriptor Descriptor { get; init; }

    internal required EntityEntry Entry { get; init; }

    internal required RevisionType RevisionType { get; init; }

    internal required List<ChangeDescriptor> Changes { get; init; }

    internal byte[]? SnapshotPayload { get; init; }
}

/// <summary>
/// Batch of captured changes for a single SaveChanges operation.
/// </summary>
public sealed class CommitCaptureBatch
{
    internal BcCommitEntity? Commit { get; set; }

    internal List<CapturedEntityChange> Changes { get; } = [];

    internal List<BcRevisionEntity> Revisions { get; } = [];

    internal List<BcDeltaEntity> Deltas { get; } = [];

    internal List<BcSnapshotEntity> Snapshots { get; } = [];
}

/// <summary>
/// Scans the EF Core change tracker for chronicle-enabled entities.
/// </summary>
public sealed class ChangeTrackerScanner(
    EntityDescriptorCache descriptorCache,
    BusinessChronicleEfOptions options,
    IChronicleSerializer serializer,
    IActorResolver actorResolver,
    IChronicleClock clock)
{
    /// <summary>
    /// Scans the change tracker and builds a capture batch.
    /// </summary>
    public async ValueTask<CommitCaptureBatch> ScanAsync(
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CommitCaptureBatch batch = new();

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is BcCommitEntity or BcRevisionEntity or BcDeltaEntity or BcSnapshotEntity or BcMetadataEntity or BcTagEntity or BcCommentEntity)
            {
                continue;
            }

            if (entry.Metadata.IsOwned() && !options.CaptureOwnedEntities)
            {
                continue;
            }

            EntityDescriptor descriptor = descriptorCache.GetOrAdd(entry.Metadata, options);
            if (!EntityFilter.ShouldCapture(descriptor, entry.State))
            {
                continue;
            }

            RevisionType revisionType = entry.State switch
            {
                EntityState.Added => RevisionType.Create,
                EntityState.Modified => RevisionType.Update,
                EntityState.Deleted => RevisionType.Delete,
                _ => RevisionType.Update,
            };

            List<ChangeDescriptor> changes = DeltaFactory.CreateChanges(entry, descriptor);
            byte[]? snapshotPayload = await SnapshotFactory.TryCreateSnapshotAsync(
                entry,
                descriptor,
                revisionType,
                options,
                serializer,
                cancellationToken).ConfigureAwait(false);

            batch.Changes.Add(new CapturedEntityChange
            {
                Descriptor = descriptor,
                Entry = entry,
                RevisionType = revisionType,
                Changes = changes,
                SnapshotPayload = snapshotPayload,
            });
        }

        if (batch.Changes.Count == 0)
        {
            return batch;
        }

        Actor actor = await actorResolver.ResolveAsync(cancellationToken).ConfigureAwait(false);
        batch.Commit = new BcCommitEntity
        {
            Id = NewChronicleId(),
            Message = "Entity changes",
            AuthorId = actor.Id,
            AuthorType = (int)actor.Type,
            AuthorDisplayName = actor.DisplayName,
            CommittedAt = clock.GetUtcNow(),
        };

        foreach (CapturedEntityChange change in batch.Changes)
        {
            string entityId = descriptorCache.GetOrAdd(change.Entry.Metadata, options).GetEntityId(change.Entry.Entity);
            string revisionId = NewChronicleId();
            BcRevisionEntity revision = new()
            {
                Id = revisionId,
                CommitId = batch.Commit.Id,
                EntityType = change.Descriptor.EntityTypeName,
                EntityId = entityId,
                RevisionType = (int)change.RevisionType,
                RevisionState = (int)RevisionState.Active,
                CreatedAt = clock.GetUtcNow(),
                VersionNumber = await GetNextVersionAsync(context, change.Descriptor.EntityTypeName, entityId, cancellationToken).ConfigureAwait(false),
            };

            batch.Revisions.Add(revision);
            batch.Deltas.AddRange(DeltaFactory.CreateEntities(revisionId, change.Changes));

            if (change.SnapshotPayload is not null)
            {
                batch.Snapshots.Add(new BcSnapshotEntity
                {
                    RevisionId = revisionId,
                    EntityType = revision.EntityType,
                    EntityId = revision.EntityId,
                    CapturedAt = clock.GetUtcNow(),
                    Payload = change.SnapshotPayload,
                    ContentType = "application/json",
                });
            }
        }

        return batch;
    }

    private static async ValueTask<long> GetNextVersionAsync(
        DbContext context,
        string entityType,
        string entityId,
        CancellationToken cancellationToken)
    {
        long current = await context.Set<BcRevisionEntity>()
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .Select(static r => (long?)r.VersionNumber)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? 0;

        return current + 1;
    }

    private static string NewChronicleId() => Guid.NewGuid().ToString("N");
}

internal static class DeltaFactory
{
    internal static List<ChangeDescriptor> CreateChanges(EntityEntry entry, EntityDescriptor descriptor)
    {
        List<ChangeDescriptor> changes = [];
        ChangeKind kind = entry.State switch
        {
            EntityState.Added => ChangeKind.Added,
            EntityState.Deleted => ChangeKind.Removed,
            _ => ChangeKind.Modified,
        };

        foreach (PropertyEntry property in entry.Properties)
        {
            if (!PropertyFilter.ShouldCapture(property.Metadata.Name, descriptor.Options))
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !property.IsModified)
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !PropertyComparer.HasChanged(property.OriginalValue, property.CurrentValue))
            {
                continue;
            }

            changes.Add(new ChangeDescriptor
            {
                Path = property.Metadata.Name,
                Kind = kind,
                PropertyChange = new PropertyChange
                {
                    PropertyName = property.Metadata.Name,
                    OldValue = PropertyValueFormatter.Format(property.OriginalValue),
                    NewValue = PropertyValueFormatter.Format(property.CurrentValue),
                    ValueType = property.Metadata.ClrType.FullName,
                },
            });
        }

        return changes;
    }

    internal static IEnumerable<BcDeltaEntity> CreateEntities(string revisionId, IReadOnlyList<ChangeDescriptor> changes)
    {
        foreach (ChangeDescriptor change in changes)
        {
            yield return new BcDeltaEntity
            {
                RevisionId = revisionId,
                Path = change.Path,
                ChangeKind = (int)change.Kind,
                PropertyName = change.PropertyChange?.PropertyName,
                OldValue = change.PropertyChange?.OldValue,
                NewValue = change.PropertyChange?.NewValue,
                ValueType = change.PropertyChange?.ValueType,
            };
        }
    }
}

internal static class SnapshotFactory
{
    internal static async ValueTask<byte[]?> TryCreateSnapshotAsync(
        EntityEntry entry,
        EntityDescriptor descriptor,
        RevisionType revisionType,
        BusinessChronicleEfOptions options,
        IChronicleSerializer serializer,
        CancellationToken cancellationToken)
    {
        if (!ShouldCaptureSnapshot(descriptor.Options.SnapshotFrequency, revisionType))
        {
            return null;
        }

        if (entry.State == EntityState.Deleted)
        {
            return null;
        }

        Dictionary<string, object?> payload = new(StringComparer.Ordinal);
        foreach (PropertyEntry property in entry.Properties)
        {
            if (descriptor.Options.IgnoredProperties.Contains(property.Metadata.Name))
            {
                continue;
            }

            payload[property.Metadata.Name] = property.CurrentValue;
        }

        Abstractions.Results.ChronicleResult<ReadOnlyMemory<byte>> serialized =
            await serializer.SerializeAsync(payload, cancellationToken).ConfigureAwait(false);

        if (serialized.IsFailure)
        {
            return null;
        }

        byte[] bytes = serialized.Value!.ToArray();
        return bytes.Length <= options.MaxSnapshotPayloadBytes ? bytes : null;
    }

    private static bool ShouldCaptureSnapshot(SnapshotFrequency frequency, RevisionType revisionType) =>
        frequency switch
        {
            SnapshotFrequency.Never => false,
            SnapshotFrequency.CreateAndDelete => revisionType is RevisionType.Create or RevisionType.Delete,
            _ => true,
        };
}

internal static class PropertyValueFormatter
{
    internal static string? Format(object? value) => value switch
    {
        null => null,
        byte[] bytes => Convert.ToBase64String(bytes),
        string text => text,
        bool boolean => boolean ? bool.TrueString : bool.FalseString,
        DateTime dateTime => dateTime.ToString("O"),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
        DateOnly dateOnly => dateOnly.ToString("O"),
        TimeOnly timeOnly => timeOnly.ToString("O"),
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
    };
}
