using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.EntityFrameworkCore.ChangeCapture;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Interceptors;
using BusinessChronicle.EntityFrameworkCore.Metadata;
using BusinessChronicle.EntityFrameworkCore.Providers;
using BusinessChronicle.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BusinessChronicle.EntityFrameworkCore.DependencyInjection;

/// <summary>
/// Dependency injection extensions for BusinessChronicle EF Core integration.
/// </summary>
public static class EntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds BusinessChronicle EF Core services.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleEntityFrameworkCore(
        this IServiceCollection services,
        Action<BusinessChronicleOptionsBuilder>? configure = null,
        IBusinessChronicleDatabaseProvider? provider = null)
    {
        BusinessChronicleEfOptions options = new();
        configure?.Invoke(new BusinessChronicleOptionsBuilder(options));
        options.ProviderName ??= provider?.Name ?? "EntityFrameworkCore";

        services.TryAddSingleton(options);
        services.TryAddSingleton<EntityDescriptorCache>();
        services.TryAddSingleton<ChangeTrackerScanner>();
        services.TryAddSingleton<BusinessChronicleSaveChangesInterceptor>();
        services.TryAddScoped<IChronicleStore>(static sp =>
        {
            DbContext? context = sp.GetService<DbContext>();
            return context is null
                ? throw new InvalidOperationException("DbContext must be registered to use EfChronicleStore.")
                : new EfChronicleStore(context);
        });

        if (provider is not null)
        {
            services.TryAddSingleton(provider);
        }

        return services;
    }
}
