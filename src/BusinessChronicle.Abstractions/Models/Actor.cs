namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Identifies an actor that performed a chronicle operation.
/// </summary>
public sealed record Actor
{
    /// <summary>
    /// Gets the stable actor identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the actor classification.
    /// </summary>
    public required Enums.ActorType Type { get; init; }

    /// <summary>
    /// Gets an optional display name for the actor.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets optional claims or attributes associated with the actor.
    /// </summary>
    public FrozenDictionary<string, string>? Claims { get; init; }
}
