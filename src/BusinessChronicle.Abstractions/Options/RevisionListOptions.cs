namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Query options for listing revisions of an entity.
/// </summary>
public sealed class RevisionListOptions
{
    /// <summary>
    /// Gets or sets the maximum number of revisions to return.
    /// </summary>
    public int MaxResults { get; set; } = 100;

    /// <summary>
    /// Gets or sets the revision state filter.
    /// </summary>
    public Enums.RevisionState? StateFilter { get; set; }

    /// <summary>
    /// Gets or sets the revision type filter.
    /// </summary>
    public Enums.RevisionType? TypeFilter { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive lower bound on <see cref="Models.Revision.CreatedAt"/>.
    /// </summary>
    public DateTimeOffset? FromUtc { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive upper bound on <see cref="Models.Revision.CreatedAt"/>.
    /// </summary>
    public DateTimeOffset? ToUtc { get; set; }
}
