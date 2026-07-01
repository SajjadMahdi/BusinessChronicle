namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Result of comparing two revisions or version pointers for an entity.
/// </summary>
public sealed record RevisionComparisonResult
{
    /// <summary>
    /// Gets the comparison target that was evaluated.
    /// </summary>
    public required Models.ComparisonTarget Target { get; init; }

    /// <summary>
    /// Gets the structural changes identified between the source and target.
    /// </summary>
    public required IReadOnlyList<Models.ChangeDescriptor> Changes { get; init; }

    /// <summary>
    /// Gets the source snapshot when one was materialized for comparison.
    /// </summary>
    public Models.Snapshot? SourceSnapshot { get; init; }

    /// <summary>
    /// Gets the target snapshot when one was materialized for comparison.
    /// </summary>
    public Models.Snapshot? TargetSnapshot { get; init; }
}
