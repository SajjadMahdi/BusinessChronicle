namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Query options for timeline and activity feed projections.
/// </summary>
public sealed class TimelineQueryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of timeline entries to return.
    /// </summary>
    public int MaxResults { get; set; } = 100;

    /// <summary>
    /// Gets or sets an optional inclusive lower bound on entry timestamps.
    /// </summary>
    public DateTimeOffset? FromUtc { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive upper bound on entry timestamps.
    /// </summary>
    public DateTimeOffset? ToUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether results are returned in descending chronological order.
    /// </summary>
    public bool Descending { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional actor filter.
    /// </summary>
    public string? ActorIdFilter { get; set; }

    /// <summary>
    /// Gets or sets an optional revision type filter.
    /// </summary>
    public Enums.RevisionType? RevisionTypeFilter { get; set; }
}
