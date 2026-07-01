namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Metadata size and shape constraints.
/// </summary>
public sealed class MetadataOptions
{
    /// <summary>
    /// Gets or sets the maximum number of metadata entries per artifact.
    /// </summary>
    public int MaxEntries { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum length of a metadata key.
    /// </summary>
    public int MaxKeyLength { get; set; } = 256;

    /// <summary>
    /// Gets or sets the maximum length of a metadata value.
    /// </summary>
    public int MaxValueLength { get; set; } = 4096;

    /// <summary>
    /// Gets or sets a value indicating whether duplicate keys are permitted.
    /// </summary>
    public bool AllowDuplicateKeys { get; set; }
}
