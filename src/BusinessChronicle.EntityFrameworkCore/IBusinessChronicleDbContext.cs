using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessChronicle.EntityFrameworkCore;

/// <summary>
/// DbContext contract required for BusinessChronicle EF Core integration.
/// </summary>
public interface IBusinessChronicleDbContext
{
    /// <summary>
    /// Gets chronicle commits.
    /// </summary>
    DbSet<BcCommitEntity> BcCommits { get; }

    /// <summary>
    /// Gets chronicle revisions.
    /// </summary>
    DbSet<BcRevisionEntity> BcRevisions { get; }

    /// <summary>
    /// Gets chronicle deltas.
    /// </summary>
    DbSet<BcDeltaEntity> BcDeltas { get; }

    /// <summary>
    /// Gets chronicle snapshots.
    /// </summary>
    DbSet<BcSnapshotEntity> BcSnapshots { get; }

    /// <summary>
    /// Gets chronicle metadata entries.
    /// </summary>
    DbSet<BcMetadataEntity> BcMetadata { get; }

    /// <summary>
    /// Gets chronicle tags.
    /// </summary>
    DbSet<BcTagEntity> BcTags { get; }

    /// <summary>
    /// Gets chronicle comments.
    /// </summary>
    DbSet<BcCommentEntity> BcComments { get; }

    /// <summary>
    /// Gets the underlying EF Core context.
    /// </summary>
    DbContext Context { get; }
}
