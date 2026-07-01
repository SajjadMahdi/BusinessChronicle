using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Engine;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Providers;
using BusinessChronicle.Storage.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BusinessChronicle.DependencyInjection;

/// <summary>
/// Dependency injection extensions for BusinessChronicle.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BusinessChronicle core services with in-memory storage defaults.
    /// </summary>
    public static IServiceCollection AddBusinessChronicle(
        this IServiceCollection services,
        Action<ChronicleOptions>? configure = null)
    {
        ChronicleOptions options = new();
        configure?.Invoke(options);
        options.Storage.ProviderName ??= "Memory";

        services.TryAddSingleton(_ => options);
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<IChronicleClock, DefaultChronicleClock>();
        services.TryAddSingleton<IChronicleSerializer, DefaultChronicleSerializer>();
        services.TryAddSingleton<IActorResolver, NullActorResolver>();
        services.TryAddSingleton<IChronicleMetadataProvider, DefaultMetadataProvider>();
        services.TryAddSingleton<IChronicleDiffEngine, DefaultDiffEngine>();
        services.TryAddSingleton<IRevisionComparer, RevisionComparer>();
        services.TryAddSingleton<IRevisionFactory, RevisionFactory>();
        services.TryAddSingleton<IChronicleStore, InMemoryChronicleStore>();
        services.TryAddSingleton<IChronicleCommitPipeline, ChronicleCommitPipeline>();
        services.TryAddSingleton<ChronicleSessionFactory>();

        return services;
    }

    /// <summary>
    /// Adds BusinessChronicle with EF Core persistence and change capture.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleEntityFrameworkCore(
        this IServiceCollection services,
        Action<ChronicleOptions>? configure = null,
        Action<BusinessChronicleOptionsBuilder>? configureEf = null,
        IBusinessChronicleDatabaseProvider? provider = null)
    {
        AddBusinessChronicle(services, configure);
        services.RemoveAll<IChronicleStore>();
        EntityFrameworkCore.DependencyInjection.EntityFrameworkCoreServiceCollectionExtensions
            .AddBusinessChronicleEntityFrameworkCore(services, configureEf, provider);
        return services;
    }
}
