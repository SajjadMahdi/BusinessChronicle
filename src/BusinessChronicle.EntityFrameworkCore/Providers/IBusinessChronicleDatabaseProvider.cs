using Microsoft.EntityFrameworkCore;

namespace BusinessChronicle.EntityFrameworkCore.Providers;

/// <summary>
/// Provider-specific BusinessChronicle model optimizations.
/// </summary>
public interface IBusinessChronicleDatabaseProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Applies provider-specific model configuration.
    /// </summary>
    void ConfigureModel(ModelBuilder modelBuilder, Configuration.BusinessChronicleEfOptions options);
}
