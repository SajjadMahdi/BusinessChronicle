namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted comment entry.
/// </summary>
public sealed class BcCommentEntity
{
    public string Id { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string AuthorId { get; set; } = string.Empty;

    public int AuthorType { get; set; }

    public string? AuthorDisplayName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CommitId { get; set; }

    public BcCommitEntity? Commit { get; set; }

    public string? RevisionId { get; set; }

    public BcRevisionEntity? Revision { get; set; }
}
