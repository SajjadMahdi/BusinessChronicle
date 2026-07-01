namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Navigates the revision version graph for an entity lineage.
/// </summary>
public interface IVersionGraph
{
    /// <summary>
    /// Gets the parent revision of the specified revision.
    /// </summary>
    /// <param name="revisionId">The child revision identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parent revision or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Revision>> GetParentAsync(
        Identifiers.RevisionId revisionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets direct child revisions of the specified revision.
    /// </summary>
    /// <param name="revisionId">The parent revision identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Child revisions or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.Revision>>> GetChildrenAsync(
        Identifiers.RevisionId revisionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ancestor revision references ordered from root to the specified revision.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Ancestor references or a failure result.</returns>
    ValueTask<Results.ChronicleResult<IReadOnlyList<Models.RevisionReference>>> GetAncestorsAsync(
        Identifiers.RevisionId revisionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active head revision for an entity lineage.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The head revision or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Revision>> GetHeadAsync(
        Models.EntityReference entity,
        CancellationToken cancellationToken = default);
}
