namespace BusinessChronicle.EntityFrameworkCore.Configuration;

/// <summary>
/// Entity Framework Core integration options.
/// </summary>
public sealed class BusinessChronicleEfOptions
{
    /// <summary>
    /// Gets or sets the table name prefix for chronicle tables.
    /// </summary>
    public string TablePrefix { get; set; } = "BC_";

    /// <summary>
    /// Gets or sets the default schema for chronicle tables.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets transaction behavior when chronicle persistence fails.
    /// </summary>
    public ChronicleTransactionBehavior TransactionBehavior { get; set; } = ChronicleTransactionBehavior.FailTransaction;

    /// <summary>
    /// Gets or sets a value indicating whether shadow properties are captured when configured.
    /// </summary>
    public bool CaptureShadowProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether owned entity types are captured.
    /// </summary>
    public bool CaptureOwnedEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether complex types are captured.
    /// </summary>
    public bool CaptureComplexTypes { get; set; } = true;

    /// <summary>
    /// Gets or sets default entity options applied by convention.
    /// </summary>
    public EntityChronicleOptions DefaultEntityOptions { get; } = new();

    /// <summary>
    /// Gets or sets the maximum snapshot payload size in bytes.
    /// </summary>
    public int MaxSnapshotPayloadBytes { get; set; } = 1_048_576;

    /// <summary>
    /// Gets or sets the storage provider name reported by EF Core integration.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether non-chronicle entity types are enabled by convention.
    /// </summary>
    public bool EnableEntitiesByConvention { get; set; }
}

/// <summary>
/// Fluent builder for <see cref="BusinessChronicleEfOptions"/>.
/// </summary>
public sealed class BusinessChronicleOptionsBuilder(BusinessChronicleEfOptions options)
{
    /// <summary>
    /// Gets the options instance being configured.
    /// </summary>
    public BusinessChronicleEfOptions Options { get; } = options;

    /// <summary>
    /// Sets the table prefix.
    /// </summary>
    public BusinessChronicleOptionsBuilder UseTablePrefix(string prefix)
    {
        Options.TablePrefix = prefix;
        return this;
    }

    /// <summary>
    /// Sets the schema.
    /// </summary>
    public BusinessChronicleOptionsBuilder UseSchema(string? schema)
    {
        Options.Schema = schema;
        return this;
    }

    /// <summary>
    /// Sets transaction behavior.
    /// </summary>
    public BusinessChronicleOptionsBuilder UseTransactionBehavior(ChronicleTransactionBehavior behavior)
    {
        Options.TransactionBehavior = behavior;
        return this;
    }

    /// <summary>
    /// Configures default entity options.
    /// </summary>
    public BusinessChronicleOptionsBuilder ConfigureDefaults(Action<EntityChronicleOptions> configure)
    {
        configure(Options.DefaultEntityOptions);
        return this;
    }
}
