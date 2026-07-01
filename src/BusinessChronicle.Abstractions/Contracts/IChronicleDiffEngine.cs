namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Computes structural differences between two revisions or versions.
/// </summary>
public interface IChronicleDiffEngine
{
    /// <summary>
    /// Computes change descriptors between a comparison target's source and target.
    /// </summary>
    /// <param name="target">The comparison target.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Change descriptors or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.ChangeDescriptor>>> DiffAsync(
        Models.ComparisonTarget target,
        CancellationToken cancellationToken = default);
}
