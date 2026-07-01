namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Describes a structural change within a revision or comparison result.
/// </summary>
public sealed record ChangeDescriptor
{
    /// <summary>
    /// Gets the JSON-pointer-style or logical path of the change.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the kind of change.
    /// </summary>
    public required Enums.ChangeKind Kind { get; init; }

    /// <summary>
    /// Gets property-level change details when applicable.
    /// </summary>
    public PropertyChange? PropertyChange { get; init; }
}
