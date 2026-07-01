namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted tag entry.
/// </summary>
public sealed class BcTagEntity
{
    public long Id { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? CommitId { get; set; }

    public BcCommitEntity? Commit { get; set; }

    public string? RevisionId { get; set; }

    public BcRevisionEntity? Revision { get; set; }
}
