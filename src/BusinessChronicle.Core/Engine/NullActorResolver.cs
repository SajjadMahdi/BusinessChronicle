using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Engine;

/// <summary>
/// Actor resolver that returns a stable system actor.
/// </summary>
public sealed class NullActorResolver : IActorResolver
{
    private static readonly Actor SystemActor = new()
    {
        Id = "system",
        Type = ActorType.System,
        DisplayName = "System",
    };

    /// <inheritdoc />
    public ValueTask<Actor> ResolveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(SystemActor);
    }
}
