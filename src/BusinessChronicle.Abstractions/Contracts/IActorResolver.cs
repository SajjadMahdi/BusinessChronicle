namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Resolves the current actor from ambient context such as HTTP, desktop session, or service identity.
/// </summary>
public interface IActorResolver
{
    /// <summary>
    /// Resolves the actor performing the current operation.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved actor.</returns>
    ValueTask<Models.Actor> ResolveAsync(CancellationToken cancellationToken = default);
}
