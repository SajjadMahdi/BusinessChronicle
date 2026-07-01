namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted metadata entry.
/// </summary>
public sealed class BcMetadataEntity
{
    public long Id { get; set; }

    public string OwnerType { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? CommitId { get; set; }

    public BcCommitEntity? Commit { get; set; }

    public string? RevisionId { get; set; }

    public BcRevisionEntity? Revision { get; set; }
}
