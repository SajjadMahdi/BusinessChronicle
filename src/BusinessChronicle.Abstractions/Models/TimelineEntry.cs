namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Chronological view model for activity feeds and timelines.
/// </summary>
public sealed record TimelineEntry
{
    /// <summary>
    /// Gets the underlying chronicle entry.
    /// </summary>
    public required ChronicleEntry Entry { get; init; }

    /// <summary>
    /// Gets the sequence number within the queried timeline window.
    /// </summary>
    public required int Sequence { get; init; }

    /// <summary>
    /// Gets the version pointer at this timeline position when available.
    /// </summary>
    public VersionPointer? Version { get; init; }
}
