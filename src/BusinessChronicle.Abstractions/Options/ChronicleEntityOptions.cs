namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Entity-level chronicle behavior settings.
/// </summary>
public sealed class ChronicleEntityOptions
{
    /// <summary>
    /// Gets or sets the logical entity type name override used for storage routing.
    /// </summary>
    public string? EntityTypeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether delete operations are tracked.
    /// </summary>
    public bool TrackDeletes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether snapshots are captured for revisions.
    /// </summary>
    public bool CaptureSnapshots { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether rollback operations are permitted for this entity.
    /// </summary>
    public bool AllowRollback { get; set; } = true;
}
