namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Supplies and enriches metadata for commits and revisions.
/// </summary>
public interface IChronicleMetadataProvider
{
    /// <summary>
    /// Gets default metadata for an entity.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Metadata for the entity.</returns>
    ValueTask<Models.ChronicleMetadata> GetMetadataAsync(
        Models.EntityReference entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches metadata using commit context.
    /// </summary>
    /// <param name="metadata">The existing metadata.</param>
    /// <param name="context">The commit context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Enriched metadata or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.ChronicleMetadata>> EnrichAsync(
        Models.ChronicleMetadata metadata,
        ICommitContext context,
        CancellationToken cancellationToken = default);
}
