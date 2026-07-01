using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;

namespace BusinessChronicle.Internal;

internal sealed class InMemoryChronicleState : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    private readonly Dictionary<string, Revision> _revisions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Snapshot> _snapshots = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Commit> _commits = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ChronicleEntry>> _entriesByEntity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _headRevisionByEntity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _versionCounterByEntity = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _childrenByRevision = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyList<ChangeDescriptor>> _changesByRevision = new(StringComparer.Ordinal);

    internal void SetRevisionChanges(RevisionId revisionId, IReadOnlyList<ChangeDescriptor> changes)
    {
        _lock.EnterWriteLock();
        try
        {
            _changesByRevision[revisionId.Value] = changes;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal IReadOnlyList<ChangeDescriptor> GetRevisionChanges(RevisionId revisionId)
    {
        _lock.EnterReadLock();
        try
        {
            return _changesByRevision.TryGetValue(revisionId.Value, out IReadOnlyList<ChangeDescriptor>? changes) && changes is not null
                ? changes
                : [];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal bool TryGetRevision(RevisionId id, out Revision revision)
    {
        _lock.EnterReadLock();
        try
        {
            if (_revisions.TryGetValue(id.Value, out Revision? found) && found is not null)
            {
                revision = found;
                return true;
            }

            revision = default!;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal bool TryGetSnapshot(RevisionId id, out Snapshot snapshot)
    {
        _lock.EnterReadLock();
        try
        {
            if (_snapshots.TryGetValue(id.Value, out Snapshot? found) && found is not null)
            {
                snapshot = found;
                return true;
            }

            snapshot = default!;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal bool TryGetCommit(CommitId id, out Commit commit)
    {
        _lock.EnterReadLock();
        try
        {
            if (_commits.TryGetValue(id.Value, out Commit? found) && found is not null)
            {
                commit = found;
                return true;
            }

            commit = default!;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal Revision? GetHeadRevision(EntityReference entity)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_headRevisionByEntity.TryGetValue(EntityKey.Create(entity), out string? headId) || headId is null)
            {
                return null;
            }

            return _revisions.TryGetValue(headId, out Revision? revision) ? revision : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal long GetNextVersion(EntityReference entity)
    {
        _lock.EnterWriteLock();
        try
        {
            string key = EntityKey.Create(entity);
            if (!_versionCounterByEntity.TryGetValue(key, out long version))
            {
                version = 0;
            }

            version++;
            _versionCounterByEntity[key] = version;
            return version;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void AddRevision(Revision revision, bool activateHead)
    {
        _lock.EnterWriteLock();
        try
        {
            _revisions[revision.Id.Value] = revision;

            if (revision.ParentRevisionId is { } parentId)
            {
                string parentKey = parentId.Value;
                if (!_childrenByRevision.TryGetValue(parentKey, out List<string>? children))
                {
                    children = [];
                    _childrenByRevision[parentKey] = children;
                }

                children.Add(revision.Id.Value);
            }

            if (activateHead && revision.State == RevisionState.Active)
            {
                SetHeadRevision(revision);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void AddSnapshot(Snapshot snapshot)
    {
        _lock.EnterWriteLock();
        try
        {
            _snapshots[snapshot.RevisionId.Value] = snapshot;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void AddCommit(Commit commit, IReadOnlyList<ChronicleEntry> entries)
    {
        _lock.EnterWriteLock();
        try
        {
            _commits[commit.Id.Value] = commit;

            foreach (ChronicleEntry entry in entries)
            {
                string entityKey = EntityKey.Create(entry.Entity);
                if (!_entriesByEntity.TryGetValue(entityKey, out List<ChronicleEntry>? list))
                {
                    list = [];
                    _entriesByEntity[entityKey] = list;
                }

                list.Add(entry);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void SupersedeRevision(RevisionId revisionId)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_revisions.TryGetValue(revisionId.Value, out Revision? revision) && revision is not null)
            {
                _revisions[revisionId.Value] = revision with { State = RevisionState.Superseded };
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void ActivateRevision(Revision revision)
    {
        _lock.EnterWriteLock();
        try
        {
            Revision active = revision with { State = RevisionState.Active };
            _revisions[revision.Id.Value] = active;
            SetHeadRevision(active);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal IReadOnlyList<RevisionReference> ListRevisionReferences(
        EntityReference entity,
        RevisionListOptions options)
    {
        _lock.EnterReadLock();
        try
        {
            List<RevisionReference> results = [];
            foreach (Revision revision in _revisions.Values)
            {
                if (!MatchesEntity(revision.Entity, entity))
                {
                    continue;
                }

                if (options.StateFilter is { } state && revision.State != state)
                {
                    continue;
                }

                if (options.TypeFilter is { } type && revision.Type != type)
                {
                    continue;
                }

                if (options.FromUtc is { } from && revision.CreatedAt < from)
                {
                    continue;
                }

                if (options.ToUtc is { } to && revision.CreatedAt > to)
                {
                    continue;
                }

                results.Add(new RevisionReference(revision.Id, revision.Entity));
            }

            results = results
                .OrderBy(reference => _revisions[reference.Id.Value].CreatedAt)
                .TakeLast(options.MaxResults)
                .ToList();

            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal IReadOnlyList<ChronicleEntry> GetEntries(EntityReference entity, TimelineQueryOptions options)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_entriesByEntity.TryGetValue(EntityKey.Create(entity), out List<ChronicleEntry>? entries) || entries is null)
            {
                return [];
            }

            IEnumerable<ChronicleEntry> query = entries;
            if (options.FromUtc is { } from)
            {
                query = query.Where(entry => entry.OccurredAt >= from);
            }

            if (options.ToUtc is { } to)
            {
                query = query.Where(entry => entry.OccurredAt <= to);
            }

            if (options.ActorIdFilter is { } actorId)
            {
                query = query.Where(entry => entry.Actor.Id == actorId);
            }

            if (options.RevisionTypeFilter is { } revisionType)
            {
                query = query.Where(entry => entry.RevisionType == revisionType);
            }

            List<ChronicleEntry> materialized = query.ToList();
            materialized.Sort(static (left, right) => left.OccurredAt.CompareTo(right.OccurredAt));

            if (options.Descending)
            {
                materialized.Reverse();
            }

            if (materialized.Count > options.MaxResults)
            {
                materialized = materialized.Take(options.MaxResults).ToList();
            }

            return materialized;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal IReadOnlyList<Revision> GetChildren(RevisionId revisionId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_childrenByRevision.TryGetValue(revisionId.Value, out List<string>? childIds) || childIds is null)
            {
                return [];
            }

            List<Revision> children = [];
            foreach (string childId in childIds)
            {
                if (_revisions.TryGetValue(childId, out Revision? revision) && revision is not null)
                {
                    children.Add(revision);
                }
            }

            return children;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal IReadOnlyList<RevisionReference> GetAncestors(RevisionId revisionId)
    {
        _lock.EnterReadLock();
        try
        {
            List<RevisionReference> ancestors = [];
            if (!_revisions.TryGetValue(revisionId.Value, out Revision? currentRevision) || currentRevision is null)
            {
                return ancestors;
            }

            Revision current = currentRevision;

            while (current.ParentRevisionId is { } parentId &&
                   _revisions.TryGetValue(parentId.Value, out Revision? parentRevision) &&
                   parentRevision is Revision parent)
            {
                ancestors.Add(new RevisionReference(parent.Id, parent.Entity));
                current = parent;
            }

            ancestors.Reverse();
            return ancestors;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal Revision? FindRevisionByVersion(EntityReference entity, VersionPointer version)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (Revision revision in _revisions.Values)
            {
                if (!MatchesEntity(revision.Entity, entity))
                {
                    continue;
                }

                if (revision.Version is not { } revisionVersion)
                {
                    continue;
                }

                if (revisionVersion.Number != version.Number)
                {
                    continue;
                }

                if (version.Label is not null &&
                    !string.Equals(revisionVersion.Label, version.Label, StringComparison.Ordinal))
                {
                    continue;
                }

                return revision;
            }

            return null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private void SetHeadRevision(Revision revision)
    {
        string entityKey = EntityKey.Create(revision.Entity);
        if (_headRevisionByEntity.TryGetValue(entityKey, out string? currentHeadId) &&
            currentHeadId is not null &&
            _revisions.TryGetValue(currentHeadId, out Revision? currentHead) &&
            currentHead is not null &&
            currentHead.Id != revision.Id)
        {
            _revisions[currentHeadId] = currentHead with { State = RevisionState.Superseded };
        }

        _headRevisionByEntity[entityKey] = revision.Id.Value;
    }

    /// <inheritdoc />
    public void Dispose() => _lock.Dispose();

    private static bool MatchesEntity(EntityReference left, EntityReference right)
    {
        if (left.Id != right.Id)
        {
            return false;
        }

        return string.Equals(left.EntityType, right.EntityType, StringComparison.Ordinal);
    }
}
