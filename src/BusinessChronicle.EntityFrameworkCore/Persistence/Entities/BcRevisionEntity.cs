namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted entity revision.
/// </summary>
public sealed class BcRevisionEntity
{
    public string Id { get; set; } = string.Empty;

    public string CommitId { get; set; } = string.Empty;

    public BcCommitEntity Commit { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? EntityDisplayName { get; set; }

    public int RevisionType { get; set; }

    public int RevisionState { get; set; }

    public string? ParentRevisionId { get; set; }

    public long VersionNumber { get; set; }

    public string? VersionLabel { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? MetadataJson { get; set; }

    public ICollection<BcDeltaEntity> Deltas { get; set; } = [];

    public BcSnapshotEntity? Snapshot { get; set; }

    public ICollection<BcMetadataEntity> Metadata { get; set; } = [];

    public ICollection<BcTagEntity> Tags { get; set; } = [];

    public ICollection<BcCommentEntity> Comments { get; set; } = [];
}
