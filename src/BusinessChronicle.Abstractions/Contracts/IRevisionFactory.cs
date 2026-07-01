namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Creates immutable <see cref="Models.Revision"/> instances from a revision context.
/// </summary>
public interface IRevisionFactory
{
    /// <summary>
    /// Creates a revision from the supplied context.
    /// </summary>
    /// <param name="context">The revision construction context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created revision or a failure result.</returns>
    ValueTask<Results.ChronicleResult<Models.Revision>> CreateAsync(
        IRevisionContext context,
        CancellationToken cancellationToken = default);
}
