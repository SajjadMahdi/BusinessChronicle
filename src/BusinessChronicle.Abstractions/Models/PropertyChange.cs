namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Describes a property-level change captured in a revision.
/// </summary>
public sealed record PropertyChange
{
    /// <summary>
    /// Gets the name of the changed property or field.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Gets the serialized previous value, if any.
    /// </summary>
    public string? OldValue { get; init; }

    /// <summary>
    /// Gets the serialized new value, if any.
    /// </summary>
    public string? NewValue { get; init; }

    /// <summary>
    /// Gets the logical type name of the property value for deserialization.
    /// </summary>
    public string? ValueType { get; init; }
}
