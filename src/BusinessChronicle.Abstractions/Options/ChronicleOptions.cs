namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Root configuration for BusinessChronicle.
/// </summary>
public sealed class ChronicleOptions
{
    /// <summary>
    /// Gets or sets entity-specific chronicle settings.
    /// </summary>
    public ChronicleEntityOptions Entity { get; set; } = new();

    /// <summary>
    /// Gets or sets snapshot capture settings.
    /// </summary>
    public SnapshotOptions Snapshot { get; set; } = new();

    /// <summary>
    /// Gets or sets storage provider settings.
    /// </summary>
    public StorageOptions Storage { get; set; } = new();

    /// <summary>
    /// Gets or sets metadata constraints and behavior.
    /// </summary>
    public MetadataOptions Metadata { get; set; } = new();
}
