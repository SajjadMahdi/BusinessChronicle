namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Higher-level revision comparison that may materialize snapshots and aggregate diff output.
/// </summary>
public interface IRevisionComparer
{
    /// <summary>
    /// Compares two revisions or version pointers for an entity.
    /// </summary>
    /// <param name="target">The comparison target.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A comparison result or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Results.RevisionComparisonResult>> CompareAsync(
        Models.ComparisonTarget target,
        CancellationToken cancellationToken = default);
}
