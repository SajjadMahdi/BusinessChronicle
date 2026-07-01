using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessChronicle.EntityFrameworkCore.Persistence;

internal static class BusinessChronicleModelConfiguration
{
    internal static void Configure(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        ConfigureCommit(modelBuilder, options);
        ConfigureRevision(modelBuilder, options);
        ConfigureDelta(modelBuilder, options);
        ConfigureSnapshot(modelBuilder, options);
        ConfigureMetadata(modelBuilder, options);
        ConfigureTag(modelBuilder, options);
        ConfigureComment(modelBuilder, options);
    }

    private static void ConfigureCommit(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcCommitEntity> entity = modelBuilder.Entity<BcCommitEntity>();
        entity.ToTable($"{options.TablePrefix}Commit", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.Id).HasMaxLength(64);
        entity.Property(static e => e.Message).HasMaxLength(4000);
        entity.Property(static e => e.ShortDescription).HasMaxLength(256);
        entity.Property(static e => e.AuthorId).HasMaxLength(256);
        entity.Property(static e => e.AuthorDisplayName).HasMaxLength(512);
        entity.Property(static e => e.ParentCommitId).HasMaxLength(64);
        entity.HasIndex(static e => e.CommittedAt);
    }

    private static void ConfigureRevision(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcRevisionEntity> entity = modelBuilder.Entity<BcRevisionEntity>();
        entity.ToTable($"{options.TablePrefix}Revision", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.Id).HasMaxLength(64);
        entity.Property(static e => e.CommitId).HasMaxLength(64);
        entity.Property(static e => e.EntityType).HasMaxLength(256);
        entity.Property(static e => e.EntityId).HasMaxLength(256);
        entity.Property(static e => e.EntityDisplayName).HasMaxLength(512);
        entity.Property(static e => e.ParentRevisionId).HasMaxLength(64);
        entity.Property(static e => e.VersionLabel).HasMaxLength(128);
        entity.HasIndex(static e => new { e.EntityType, e.EntityId, e.CreatedAt });
        entity.HasIndex(static e => new { e.EntityType, e.EntityId, e.VersionNumber });
        entity.HasOne(static e => e.Commit).WithMany(static c => c.Revisions).HasForeignKey(static e => e.CommitId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureDelta(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcDeltaEntity> entity = modelBuilder.Entity<BcDeltaEntity>();
        entity.ToTable($"{options.TablePrefix}Delta", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.RevisionId).HasMaxLength(64);
        entity.Property(static e => e.Path).HasMaxLength(1024);
        entity.Property(static e => e.PropertyName).HasMaxLength(256);
        entity.Property(static e => e.ValueType).HasMaxLength(512);
        entity.HasOne(static e => e.Revision).WithMany(static r => r.Deltas).HasForeignKey(static e => e.RevisionId).OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(static e => e.RevisionId);
    }

    private static void ConfigureSnapshot(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcSnapshotEntity> entity = modelBuilder.Entity<BcSnapshotEntity>();
        entity.ToTable($"{options.TablePrefix}Snapshot", options.Schema);
        entity.HasKey(static e => e.RevisionId);
        entity.Property(static e => e.RevisionId).HasMaxLength(64);
        entity.Property(static e => e.EntityType).HasMaxLength(256);
        entity.Property(static e => e.EntityId).HasMaxLength(256);
        entity.Property(static e => e.ContentType).HasMaxLength(128);
        entity.HasOne(static e => e.Revision).WithOne(static r => r.Snapshot).HasForeignKey<BcSnapshotEntity>(static e => e.RevisionId).OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(static e => new { e.EntityType, e.EntityId });
    }

    private static void ConfigureMetadata(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcMetadataEntity> entity = modelBuilder.Entity<BcMetadataEntity>();
        entity.ToTable($"{options.TablePrefix}Metadata", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.OwnerType).HasMaxLength(64);
        entity.Property(static e => e.OwnerId).HasMaxLength(64);
        entity.Property(static e => e.Key).HasMaxLength(256);
        entity.Property(static e => e.Value).HasMaxLength(4096);
        entity.HasIndex(static e => new { e.OwnerType, e.OwnerId });
    }

    private static void ConfigureTag(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcTagEntity> entity = modelBuilder.Entity<BcTagEntity>();
        entity.ToTable($"{options.TablePrefix}Tag", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.OwnerId).HasMaxLength(64);
        entity.Property(static e => e.Name).HasMaxLength(128);
        entity.Property(static e => e.Value).HasMaxLength(512);
        entity.HasIndex(static e => new { e.OwnerId, e.Name });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        EntityTypeBuilder<BcCommentEntity> entity = modelBuilder.Entity<BcCommentEntity>();
        entity.ToTable($"{options.TablePrefix}Comment", options.Schema);
        entity.HasKey(static e => e.Id);
        entity.Property(static e => e.Id).HasMaxLength(64);
        entity.Property(static e => e.OwnerId).HasMaxLength(64);
        entity.Property(static e => e.Text).HasMaxLength(8000);
        entity.Property(static e => e.AuthorId).HasMaxLength(256);
        entity.Property(static e => e.AuthorDisplayName).HasMaxLength(512);
        entity.HasIndex(static e => e.OwnerId);
    }
}
