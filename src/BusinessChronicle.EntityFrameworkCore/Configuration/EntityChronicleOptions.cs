namespace BusinessChronicle.EntityFrameworkCore.Configuration;

/// <summary>
/// Per-entity BusinessChronicle configuration.
/// </summary>
public sealed class EntityChronicleOptions
{
    internal const string AnnotationName = "BusinessChronicle:EntityOptions";

    /// <summary>
    /// Gets or sets a value indicating whether chronicle tracking is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets property names excluded from delta capture.
    /// </summary>
    public HashSet<string> IgnoredProperties { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets navigation property names excluded from related capture.
    /// </summary>
    public HashSet<string> ExcludedNavigations { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets navigation property names included for related capture.
    /// </summary>
    public HashSet<string> IncludedRelated { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets snapshot capture frequency.
    /// </summary>
    public SnapshotFrequency SnapshotFrequency { get; set; } = SnapshotFrequency.EveryRevision;

    /// <summary>
    /// Gets default metadata attached to revisions for this entity type.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a copy of the options.
    /// </summary>
    internal EntityChronicleOptions Clone()
    {
        EntityChronicleOptions clone = new() { Enabled = Enabled, SnapshotFrequency = SnapshotFrequency };
        clone.IgnoredProperties.UnionWith(IgnoredProperties);
        clone.ExcludedNavigations.UnionWith(ExcludedNavigations);
        clone.IncludedRelated.UnionWith(IncludedRelated);
        foreach (KeyValuePair<string, string> pair in Metadata)
        {
            clone.Metadata[pair.Key] = pair.Value;
        }

        return clone;
    }
}
