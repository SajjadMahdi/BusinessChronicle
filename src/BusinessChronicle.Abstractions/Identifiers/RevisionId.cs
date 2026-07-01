namespace BusinessChronicle.Abstractions.Identifiers;

/// <summary>
/// Strongly-typed identifier for an immutable entity revision within the chronicle.
/// </summary>
/// <param name="Value">The canonical string representation of the revision identifier.</param>
public readonly record struct RevisionId(string Value)
{
    /// <summary>
    /// Gets a value indicating whether this identifier is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Returns the string representation of this identifier.
    /// </summary>
    public override string ToString() => Value;
}
