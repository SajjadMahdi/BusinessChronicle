using BusinessChronicle.Engine;
using BusinessChronicle.EntityFrameworkCore.ChangeCapture;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Conventions;
using BusinessChronicle.EntityFrameworkCore.Interceptors;
using BusinessChronicle.EntityFrameworkCore.Metadata;
using BusinessChronicle.EntityFrameworkCore.Persistence;
using BusinessChronicle.EntityFrameworkCore.Providers;
using BusinessChronicle.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.EntityFrameworkCore.Extensions;

/// <summary>
/// <see cref="ModelBuilder"/> extensions for BusinessChronicle.
/// </summary>
public static class BusinessChronicleModelBuilderExtensions
{
    /// <summary>
    /// Configures BusinessChronicle persistence tables and conventions.
    /// </summary>
    public static ModelBuilder ConfigureBusinessChronicleDefaults(
        this ModelBuilder modelBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null,
        IBusinessChronicleDatabaseProvider? provider = null)
    {
        BusinessChronicleEfOptions options = new();
        configure?.Invoke(new BusinessChronicleOptionsBuilder(options));
        BusinessChronicleModelConfiguration.Configure(modelBuilder, options);
        provider?.ConfigureModel(modelBuilder, options);
        modelBuilder.Model.SetAnnotation(BusinessChronicleModelAnnotations.Options, options);
        modelBuilder.ConfigureBusinessChronicleConventions(options);
        return modelBuilder;
    }
}

/// <summary>
/// <see cref="EntityTypeBuilder"/> extensions for BusinessChronicle.
/// </summary>
public static class BusinessChronicleEntityTypeBuilderExtensions
{
    /// <summary>
    /// Enables BusinessChronicle change capture for the entity type.
    /// </summary>
    public static EntityTypeBuilder<TEntity> EnableBusinessChronicle<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Action<EntityChronicleOptions>? configure = null)
        where TEntity : class
    {
        EntityChronicleOptions options = new() { Enabled = true };
        configure?.Invoke(options);
        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    /// <summary>
    /// Ignores properties during delta capture.
    /// </summary>
    public static EntityTypeBuilder<TEntity> IgnoreProperties<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        params string[] propertyNames)
        where TEntity : class
    {
        EntityChronicleOptions options = GetOrCreateOptions(builder);
        foreach (string propertyName in propertyNames)
        {
            options.IgnoredProperties.Add(propertyName);
        }

        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    /// <summary>
    /// Sets snapshot capture frequency.
    /// </summary>
    public static EntityTypeBuilder<TEntity> SnapshotFrequency<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        SnapshotFrequency frequency)
        where TEntity : class
    {
        EntityChronicleOptions options = GetOrCreateOptions(builder);
        options.SnapshotFrequency = frequency;
        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    /// <summary>
    /// Adds default metadata for revisions of this entity type.
    /// </summary>
    public static EntityTypeBuilder<TEntity> Metadata<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Action<Dictionary<string, string>> configure)
        where TEntity : class
    {
        EntityChronicleOptions options = GetOrCreateOptions(builder);
        configure(options.Metadata);
        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    /// <summary>
    /// Includes related navigations in capture.
    /// </summary>
    public static EntityTypeBuilder<TEntity> IncludeRelated<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        params string[] navigationNames)
        where TEntity : class
    {
        EntityChronicleOptions options = GetOrCreateOptions(builder);
        foreach (string navigation in navigationNames)
        {
            options.IncludedRelated.Add(navigation);
        }

        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    /// <summary>
    /// Excludes navigations from capture.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ExcludeNavigation<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        params string[] navigationNames)
        where TEntity : class
    {
        EntityChronicleOptions options = GetOrCreateOptions(builder);
        foreach (string navigation in navigationNames)
        {
            options.ExcludedNavigations.Add(navigation);
        }

        EntityDiscovery.SetEntityOptions(builder.Metadata, options);
        return builder;
    }

    private static EntityChronicleOptions GetOrCreateOptions<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class =>
        EntityDiscovery.GetEntityOptions(builder.Metadata)?.Clone() ?? new EntityChronicleOptions { Enabled = true };
}

/// <summary>
/// <see cref="DbContext"/> extensions for BusinessChronicle.
/// </summary>
public static class BusinessChronicleDbContextExtensions
{
    /// <summary>
    /// Gets the EF Core chronicle store for the context.
    /// </summary>
    public static EfChronicleStore GetBusinessChronicleStore(this DbContext context) => new(context);

    /// <summary>
    /// Warms BusinessChronicle descriptor caches for the context model.
    /// </summary>
    public static void WarmBusinessChronicleCaches(this DbContext context, BusinessChronicleEfOptions options)
    {
        EntityDescriptorCache cache = new();
        cache.Warmup(context.Model, options);
    }
}

/// <summary>
/// <see cref="DbContextOptionsBuilder"/> extensions for BusinessChronicle.
/// </summary>
public static class BusinessChronicleDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds BusinessChronicle interceptors and services to EF Core.
    /// </summary>
    public static DbContextOptionsBuilder UseBusinessChronicle(
        this DbContextOptionsBuilder optionsBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null)
    {
        BusinessChronicleEfOptions options = new();
        configure?.Invoke(new BusinessChronicleOptionsBuilder(options));
        return optionsBuilder.AddInterceptors(CreateInterceptor(options));
    }

    /// <summary>
    /// Adds BusinessChronicle interceptors resolved from dependency injection.
    /// </summary>
    public static DbContextOptionsBuilder UseBusinessChronicle(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<BusinessChronicleSaveChangesInterceptor>());
    }

    internal static BusinessChronicleSaveChangesInterceptor CreateInterceptor(BusinessChronicleEfOptions options) =>
        new(
            new ChangeTrackerScanner(
                new EntityDescriptorCache(),
                options,
                new DefaultChronicleSerializer(),
                new NullActorResolver(),
                new DefaultChronicleClock(TimeProvider.System)),
            options);
}
