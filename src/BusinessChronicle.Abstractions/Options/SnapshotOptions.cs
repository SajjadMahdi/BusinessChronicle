namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Snapshot capture and retention settings.
/// </summary>
public sealed class SnapshotOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether snapshot payloads are compressed at rest.
    /// </summary>
    public bool CompressPayload { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed snapshot payload size in bytes.
    /// </summary>
    public int MaxPayloadBytes { get; set; } = 1_048_576;

    /// <summary>
    /// Gets or sets the default content type for serialized snapshots.
    /// </summary>
    public string DefaultContentType { get; set; } = "application/json";
}
