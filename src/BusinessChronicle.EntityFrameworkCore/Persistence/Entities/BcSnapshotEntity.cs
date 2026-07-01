namespace BusinessChronicle.EntityFrameworkCore.Persistence.Entities;

/// <summary>
/// Persisted entity snapshot payload.
/// </summary>
public sealed class BcSnapshotEntity
{
    public string RevisionId { get; set; } = string.Empty;

    public BcRevisionEntity Revision { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public DateTimeOffset CapturedAt { get; set; }

    public byte[] Payload { get; set; } = [];

    public string? ContentType { get; set; }

    public string? MetadataJson { get; set; }
}
