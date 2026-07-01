namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted structural delta for a revision.
/// </summary>
public sealed class BcDeltaEntity
{
    public long Id { get; set; }

    public string RevisionId { get; set; } = string.Empty;

    public BcRevisionEntity Revision { get; set; } = null!;

    public string Path { get; set; } = string.Empty;

    public int ChangeKind { get; set; }

    public string? PropertyName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ValueType { get; set; }
}
