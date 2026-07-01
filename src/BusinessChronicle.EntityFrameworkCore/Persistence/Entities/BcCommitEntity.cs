namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted chronicle commit.
/// </summary>
public sealed class BcCommitEntity
{
    public string Id { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string AuthorId { get; set; } = string.Empty;

    public int AuthorType { get; set; }

    public string? AuthorDisplayName { get; set; }

    public DateTimeOffset CommittedAt { get; set; }

    public string? ParentCommitId { get; set; }

    public string? MetadataJson { get; set; }

    public ICollection<BcRevisionEntity> Revisions { get; set; } = [];

    public ICollection<BcMetadataEntity> Metadata { get; set; } = [];

    public ICollection<BcTagEntity> Tags { get; set; } = [];

    public ICollection<BcCommentEntity> Comments { get; set; } = [];
}
