using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessChronicle.EntityFrameworkCore.Internal;

internal sealed class BusinessChronicleDbContextAdapter(DbContext context) : IBusinessChronicleDbContext
{
    public DbSet<BcCommitEntity> BcCommits => Context.Set<BcCommitEntity>();

    public DbSet<BcRevisionEntity> BcRevisions => Context.Set<BcRevisionEntity>();

    public DbSet<BcDeltaEntity> BcDeltas => Context.Set<BcDeltaEntity>();

    public DbSet<BcSnapshotEntity> BcSnapshots => Context.Set<BcSnapshotEntity>();

    public DbSet<BcMetadataEntity> BcMetadata => Context.Set<BcMetadataEntity>();

    public DbSet<BcTagEntity> BcTags => Context.Set<BcTagEntity>();

    public DbSet<BcCommentEntity> BcComments => Context.Set<BcCommentEntity>();

    public DbContext Context { get; } = context;
}
