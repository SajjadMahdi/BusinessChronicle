namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Identifies a business entity within the chronicle.
/// </summary>
public sealed record EntityReference
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public required Identifiers.EntityId Id { get; init; }

    /// <summary>
    /// Gets the logical entity type name used for routing and storage mapping.
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Gets an optional human-readable label for presentation.
    /// </summary>
    public string? DisplayName { get; init; }
}
