using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.EntityFrameworkCore.Internal;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessChronicle.EntityFrameworkCore.Storage;

/// <summary>
/// EF Core chronicle store implementation.
/// </summary>
public sealed class EfChronicleStore : IChronicleStore
{
    private readonly IBusinessChronicleDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfChronicleStore"/> class.
    /// </summary>
    public EfChronicleStore(DbContext context)
    {
        _context = new BusinessChronicleDbContextAdapter(context);
        RevisionReader = new EfRevisionReader(_context);
        RevisionWriter = new EfRevisionWriter(_context);
        Query = new EfChronicleQuery(_context);
        VersionGraph = new EfVersionGraph(_context);
    }

    /// <inheritdoc />
    public IRevisionReader RevisionReader { get; }

    /// <inheritdoc />
    public IRevisionWriter RevisionWriter { get; }

    /// <inheritdoc />
    public IChronicleQuery Query { get; }

    /// <inheritdoc />
    public IVersionGraph VersionGraph { get; }
}

internal sealed class EfRevisionReader(IBusinessChronicleDbContext context) : IRevisionReader
{
    public async ValueTask<ChronicleResult<Revision>> ReadAsync(RevisionId id, CancellationToken cancellationToken = default)
    {
        BcRevisionEntity? entity = await context.BcRevisions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id.Value, cancellationToken)
            .ConfigureAwait(false);

        return entity is null
            ? ChronicleResults.Failure<Revision>(new ChronicleError(ChronicleErrorCode.NotFound, $"Revision '{id}' was not found."))
            : ChronicleResults.Success(EfEntityMapper.ToRevision(entity));
    }

    public async ValueTask<ChronicleResult<Snapshot>> ReadSnapshotAsync(RevisionId id, CancellationToken cancellationToken = default)
    {
        BcSnapshotEntity? entity = await context.BcSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RevisionId == id.Value, cancellationToken)
            .ConfigureAwait(false);

        return entity is null
            ? ChronicleResults.Failure<Snapshot>(new ChronicleError(ChronicleErrorCode.NotFound, $"Snapshot for revision '{id}' was not found."))
            : ChronicleResults.Success(EfEntityMapper.ToSnapshot(entity));
    }

    public async ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> ListAsync(
        EntityReference entity,
        RevisionListOptions options,
        CancellationToken cancellationToken = default)
    {
        IQueryable<BcRevisionEntity> query = context.BcRevisions.AsNoTracking()
            .Where(r => r.EntityId == entity.Id.Value);

        if (!string.IsNullOrWhiteSpace(entity.EntityType))
        {
            query = query.Where(r => r.EntityType == entity.EntityType);
        }

        if (options.StateFilter is { } state)
        {
            query = query.Where(r => r.RevisionState == (int)state);
        }

        if (options.TypeFilter is { } type)
        {
            query = query.Where(r => r.RevisionType == (int)type);
        }

        if (options.FromUtc is { } from)
        {
            query = query.Where(r => r.CreatedAt >= from);
        }

        if (options.ToUtc is { } to)
        {
            query = query.Where(r => r.CreatedAt <= to);
        }

        List<BcRevisionEntity> items = await query
            .OrderBy(r => r.CreatedAt)
            .Take(options.MaxResults)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        RevisionReference[] references = items
            .Select(static r => new RevisionReference(new RevisionId(r.Id), new EntityReference
            {
                Id = new EntityId(r.EntityId),
                EntityType = r.EntityType,
                DisplayName = r.EntityDisplayName,
            }))
            .ToArray();

        return ChronicleResults.Success<IReadOnlyList<RevisionReference>>(references);
    }
}

internal sealed class EfRevisionWriter(IBusinessChronicleDbContext context) : IRevisionWriter
{
    public async ValueTask<ChronicleResult<Revision>> WriteAsync(Revision revision, CancellationToken cancellationToken = default)
    {
        BcRevisionEntity entity = EfEntityMapper.ToRevisionEntity(revision);
        context.BcRevisions.Add(entity);
        await context.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ChronicleResults.Success(revision);
    }

    public async ValueTask<ChronicleResult<Snapshot>> WriteSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        BcSnapshotEntity entity = EfEntityMapper.ToSnapshotEntity(snapshot);
        context.BcSnapshots.Add(entity);
        await context.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ChronicleResults.Success(snapshot);
    }
}

internal sealed class EfChronicleQuery(IBusinessChronicleDbContext context) : IChronicleQuery
{
    public async ValueTask<ChronicleResult<IReadOnlyList<TimelineEntry>>> GetTimelineAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        ChronicleResult<IReadOnlyList<ChronicleEntry>> entries = await GetEntriesAsync(entity, options, cancellationToken).ConfigureAwait(false);
        if (entries.IsFailure)
        {
            return ChronicleResults.Failure<IReadOnlyList<TimelineEntry>>(entries.Error!.Value);
        }

        List<TimelineEntry> timeline = [];
        for (int index = 0; index < entries.Value!.Count; index++)
        {
            timeline.Add(new TimelineEntry { Entry = entries.Value[index], Sequence = index + 1 });
        }

        return ChronicleResults.Success<IReadOnlyList<TimelineEntry>>(timeline);
    }

    public async ValueTask<ChronicleResult<IReadOnlyList<ChronicleEntry>>> GetEntriesAsync(
        EntityReference entity,
        TimelineQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        IQueryable<BcRevisionEntity> query = context.BcRevisions.AsNoTracking()
            .Include(static r => r.Deltas)
            .Where(r => r.EntityId == entity.Id.Value);

        if (!string.IsNullOrWhiteSpace(entity.EntityType))
        {
            query = query.Where(r => r.EntityType == entity.EntityType);
        }

        if (options.FromUtc is { } from)
        {
            query = query.Where(r => r.CreatedAt >= from);
        }

        if (options.ToUtc is { } to)
        {
            query = query.Where(r => r.CreatedAt <= to);
        }

        if (options.RevisionTypeFilter is { } type)
        {
            query = query.Where(r => r.RevisionType == (int)type);
        }

        query = options.Descending
            ? query.OrderByDescending(static r => r.CreatedAt)
            : query.OrderBy(static r => r.CreatedAt);

        List<BcRevisionEntity> revisions = await query.Take(options.MaxResults).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<ChronicleEntry> entries = revisions.ConvertAll(EfEntityMapper.ToChronicleEntry);
        return ChronicleResults.Success<IReadOnlyList<ChronicleEntry>>(entries);
    }

    public async ValueTask<ChronicleResult<Commit>> GetCommitAsync(CommitId id, CancellationToken cancellationToken = default)
    {
        BcCommitEntity? entity = await context.BcCommits.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id.Value, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? ChronicleResults.Failure<Commit>(new ChronicleError(ChronicleErrorCode.NotFound, $"Commit '{id}' was not found."))
            : ChronicleResults.Success(EfEntityMapper.ToCommit(entity));
    }
}

internal sealed class EfVersionGraph(IBusinessChronicleDbContext context) : IVersionGraph
{
    public async ValueTask<ChronicleResult<Revision>> GetParentAsync(RevisionId revisionId, CancellationToken cancellationToken = default)
    {
        BcRevisionEntity? entity = await context.BcRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == revisionId.Value, cancellationToken).ConfigureAwait(false);
        if (entity?.ParentRevisionId is not { } parentId)
        {
            return ChronicleResults.Failure<Revision>(new ChronicleError(ChronicleErrorCode.NotFound, "Parent revision was not found."));
        }

        BcRevisionEntity? parent = await context.BcRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == parentId, cancellationToken).ConfigureAwait(false);
        return parent is null
            ? ChronicleResults.Failure<Revision>(new ChronicleError(ChronicleErrorCode.NotFound, "Parent revision was not found."))
            : ChronicleResults.Success(EfEntityMapper.ToRevision(parent));
    }

    public async ValueTask<ChronicleResult<IReadOnlyList<Revision>>> GetChildrenAsync(RevisionId revisionId, CancellationToken cancellationToken = default)
    {
        List<BcRevisionEntity> children = await context.BcRevisions.AsNoTracking()
            .Where(r => r.ParentRevisionId == revisionId.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return ChronicleResults.Success<IReadOnlyList<Revision>>(children.ConvertAll(EfEntityMapper.ToRevision));
    }

    public async ValueTask<ChronicleResult<IReadOnlyList<RevisionReference>>> GetAncestorsAsync(RevisionId revisionId, CancellationToken cancellationToken = default)
    {
        List<RevisionReference> ancestors = [];
        BcRevisionEntity? current = await context.BcRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == revisionId.Value, cancellationToken).ConfigureAwait(false);
        while (current?.ParentRevisionId is { } parentId)
        {
            current = await context.BcRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == parentId, cancellationToken).ConfigureAwait(false);
            if (current is not null)
            {
                ancestors.Add(new RevisionReference(new RevisionId(current.Id), new EntityReference
                {
                    Id = new EntityId(current.EntityId),
                    EntityType = current.EntityType,
                    DisplayName = current.EntityDisplayName,
                }));
            }
        }

        ancestors.Reverse();
        return ChronicleResults.Success<IReadOnlyList<RevisionReference>>(ancestors);
    }

    public async ValueTask<ChronicleResult<Revision>> GetHeadAsync(EntityReference entity, CancellationToken cancellationToken = default)
    {
        IQueryable<BcRevisionEntity> query = context.BcRevisions.AsNoTracking()
            .Where(r => r.EntityId == entity.Id.Value && r.RevisionState == (int)RevisionState.Active);

        if (!string.IsNullOrWhiteSpace(entity.EntityType))
        {
            query = query.Where(r => r.EntityType == entity.EntityType);
        }

        BcRevisionEntity? head = await query.OrderByDescending(static r => r.VersionNumber).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return head is null
            ? ChronicleResults.Failure<Revision>(new ChronicleError(ChronicleErrorCode.NotFound, "Head revision was not found."))
            : ChronicleResults.Success(EfEntityMapper.ToRevision(head));
    }
}

internal static class EfEntityMapper
{
    internal static Revision ToRevision(BcRevisionEntity entity) => new()
    {
        Id = new RevisionId(entity.Id),
        CommitId = new CommitId(entity.CommitId),
        Entity = new EntityReference { Id = new EntityId(entity.EntityId), EntityType = entity.EntityType, DisplayName = entity.EntityDisplayName },
        Type = (RevisionType)entity.RevisionType,
        State = (RevisionState)entity.RevisionState,
        ParentRevisionId = entity.ParentRevisionId is null ? null : new RevisionId(entity.ParentRevisionId),
        Version = new VersionPointer { Number = entity.VersionNumber, Label = entity.VersionLabel },
        CreatedAt = entity.CreatedAt,
    };

    internal static Snapshot ToSnapshot(BcSnapshotEntity entity) => new()
    {
        RevisionId = new RevisionId(entity.RevisionId),
        Entity = new EntityReference { Id = new EntityId(entity.EntityId), EntityType = entity.EntityType },
        CapturedAt = entity.CapturedAt,
        Payload = entity.Payload,
        ContentType = entity.ContentType,
    };

    internal static Commit ToCommit(BcCommitEntity entity) => new()
    {
        Id = new CommitId(entity.Id),
        Message = new CommitMessage { Text = entity.Message, ShortDescription = entity.ShortDescription },
        Author = new Actor { Id = entity.AuthorId, Type = (ActorType)entity.AuthorType, DisplayName = entity.AuthorDisplayName },
        CommittedAt = entity.CommittedAt,
        ParentCommitId = entity.ParentCommitId is null ? null : new CommitId(entity.ParentCommitId),
    };

    internal static ChronicleEntry ToChronicleEntry(BcRevisionEntity entity) => new()
    {
        RevisionId = new RevisionId(entity.Id),
        CommitId = new CommitId(entity.CommitId),
        Entity = new EntityReference { Id = new EntityId(entity.EntityId), EntityType = entity.EntityType, DisplayName = entity.EntityDisplayName },
        Actor = new Actor { Id = "system", Type = ActorType.System },
        OccurredAt = entity.CreatedAt,
        Message = new CommitMessage { Text = "Entity changes" },
        RevisionType = (RevisionType)entity.RevisionType,
        Changes = entity.Deltas.Select(static d => new ChangeDescriptor
        {
            Path = d.Path,
            Kind = (ChangeKind)d.ChangeKind,
            PropertyChange = d.PropertyName is null ? null : new PropertyChange
            {
                PropertyName = d.PropertyName,
                OldValue = d.OldValue,
                NewValue = d.NewValue,
                ValueType = d.ValueType,
            },
        }).ToArray(),
    };

    internal static BcRevisionEntity ToRevisionEntity(Revision revision) => new()
    {
        Id = revision.Id.Value,
        CommitId = revision.CommitId.Value,
        EntityType = revision.Entity.EntityType ?? revision.Entity.Id.Value,
        EntityId = revision.Entity.Id.Value,
        EntityDisplayName = revision.Entity.DisplayName,
        RevisionType = (int)revision.Type,
        RevisionState = (int)revision.State,
        ParentRevisionId = revision.ParentRevisionId?.Value,
        VersionNumber = revision.Version?.Number ?? 1,
        VersionLabel = revision.Version?.Label,
        CreatedAt = revision.CreatedAt,
    };

    internal static BcSnapshotEntity ToSnapshotEntity(Snapshot snapshot) => new()
    {
        RevisionId = snapshot.RevisionId.Value,
        EntityType = snapshot.Entity.EntityType ?? snapshot.Entity.Id.Value,
        EntityId = snapshot.Entity.Id.Value,
        CapturedAt = snapshot.CapturedAt,
        Payload = snapshot.Payload.ToArray(),
        ContentType = snapshot.ContentType,
    };
}
