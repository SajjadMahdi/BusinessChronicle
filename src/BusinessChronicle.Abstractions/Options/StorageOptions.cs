namespace BusinessChronicle.Abstractions.Options;

/// <summary>
/// Storage provider configuration shared by all backends.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Gets or sets the logical storage provider name (for example, <c>SqlServer</c>, <c>Cosmos</c>, <c>Memory</c>).
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Gets or sets the default operation timeout.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the connection string or connection reference consumed by the selected provider.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets provider-specific settings that are opaque to the core engine.
    /// </summary>
    public FrozenDictionary<string, string> ProviderSettings { get; set; } =
        FrozenDictionary<string, string>.Empty;
}
