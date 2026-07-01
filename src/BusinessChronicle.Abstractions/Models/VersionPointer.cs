namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Sequential or labeled version pointer for an entity lineage.
/// </summary>
public readonly record struct VersionPointer
{
    /// <summary>
    /// Gets the monotonically increasing version number within an entity lineage.
    /// </summary>
    public long Number { get; init; }

    /// <summary>
    /// Gets an optional semantic label such as a branch or tag name.
    /// </summary>
    public string? Label { get; init; }
}
