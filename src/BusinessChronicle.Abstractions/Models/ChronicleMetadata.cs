namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Extensible key-value metadata attached to chronicle artifacts.
/// </summary>
public sealed record ChronicleMetadata
{
    /// <summary>
    /// Gets an empty metadata instance.
    /// </summary>
    public static ChronicleMetadata Empty { get; } = new(FrozenDictionary<string, string>.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="ChronicleMetadata"/> record.
    /// </summary>
    /// <param name="values">The metadata values.</param>
    public ChronicleMetadata(FrozenDictionary<string, string> values)
    {
        Values = values;
    }

    /// <summary>
    /// Gets the metadata values.
    /// </summary>
    public FrozenDictionary<string, string> Values { get; init; }
}
