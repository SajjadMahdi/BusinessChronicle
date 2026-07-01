using System.Collections.Concurrent;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BusinessChronicle.EntityFrameworkCore.Metadata;

/// <summary>
/// Describes a tracked property for change capture.
/// </summary>
public sealed class PropertyDescriptor
{
    internal PropertyDescriptor(IProperty property, Func<object, object?> getCurrent, Func<object, object?>? getOriginal)
    {
        Name = property.Name;
        ClrType = property.ClrType;
        IsShadow = property.IsShadowProperty();
        IsConcurrencyToken = property.IsConcurrencyToken;
        IsPrimaryKey = property.IsPrimaryKey();
        IsTemporary = property.IsPrimaryKey() && property.ValueGenerated == ValueGenerated.OnAdd;
        GetCurrentValue = getCurrent;
        GetOriginalValue = getOriginal;
    }

    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the CLR type.
    /// </summary>
    public Type ClrType { get; }

    /// <summary>
    /// Gets a value indicating whether this is a shadow property.
    /// </summary>
    public bool IsShadow { get; }

    /// <summary>
    /// Gets a value indicating whether this is a concurrency token.
    /// </summary>
    public bool IsConcurrencyToken { get; }

    /// <summary>
    /// Gets a value indicating whether this is a primary key member.
    /// </summary>
    public bool IsPrimaryKey { get; }

    /// <summary>
    /// Gets a value indicating whether the key is temporary.
    /// </summary>
    public bool IsTemporary { get; }

    internal Func<object, object?> GetCurrentValue { get; }

    internal Func<object, object?>? GetOriginalValue { get; }
}

/// <summary>
/// Describes a tracked entity type for change capture.
/// </summary>
public sealed class EntityDescriptor
{
    internal EntityDescriptor(
        IReadOnlyEntityType entityType,
        EntityChronicleOptions options,
        IReadOnlyList<PropertyDescriptor> properties,
        Func<object, string> getEntityId,
        Func<object, string?>? getDisplayName)
    {
        ClrType = entityType.ClrType;
        EntityTypeName = entityType.DisplayName();
        Options = options;
        Properties = properties;
        GetEntityId = getEntityId;
        GetDisplayName = getDisplayName;
    }

    /// <summary>
    /// Gets the CLR type.
    /// </summary>
    public Type ClrType { get; }

    /// <summary>
    /// Gets the entity type name.
    /// </summary>
    public string EntityTypeName { get; }

    /// <summary>
    /// Gets entity options.
    /// </summary>
    public EntityChronicleOptions Options { get; }

    /// <summary>
    /// Gets tracked properties.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }

    internal Func<object, string> GetEntityId { get; }

    internal Func<object, string?>? GetDisplayName { get; }
}

/// <summary>
/// Builds and caches entity descriptors from EF Core metadata.
/// </summary>
public sealed class EntityDescriptorCache
{
    private readonly ConcurrentDictionary<Type, EntityDescriptor> _cache = new();

    /// <summary>
    /// Gets or builds a descriptor for an entity type.
    /// </summary>
    public EntityDescriptor GetOrAdd(IReadOnlyEntityType entityType, BusinessChronicleEfOptions efOptions)
    {
        Type clrType = entityType.ClrType;
        return _cache.GetOrAdd(clrType, _ => BuildDescriptor(entityType, efOptions));
    }

    /// <summary>
    /// Warms the cache for all enabled entity types in the model.
    /// </summary>
    public void Warmup(IModel model, BusinessChronicleEfOptions efOptions)
    {
        foreach (IEntityType entityType in model.GetEntityTypes())
        {
            if (entityType.ClrType == typeof(Persistence.Entities.BcCommitEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcRevisionEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcDeltaEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcSnapshotEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcMetadataEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcTagEntity) ||
                entityType.ClrType == typeof(Persistence.Entities.BcCommentEntity))
            {
                continue;
            }

            EntityChronicleOptions? options = EntityDiscovery.GetEntityOptions(entityType);
            if (options is { Enabled: true })
            {
                GetOrAdd(entityType, efOptions);
            }
        }
    }

    private static EntityDescriptor BuildDescriptor(IReadOnlyEntityType entityType, BusinessChronicleEfOptions efOptions)
    {
        EntityChronicleOptions options = EntityDiscovery.GetEntityOptions(entityType) ?? efOptions.DefaultEntityOptions.Clone();
        List<PropertyDescriptor> properties = [];

        foreach (IProperty property in entityType.GetProperties())
        {
            if (options.IgnoredProperties.Contains(property.Name))
            {
                continue;
            }

            if (property.IsShadowProperty() && !efOptions.CaptureShadowProperties)
            {
                continue;
            }

            properties.Add(PropertyMetadataCache.CreateDescriptor(property));
        }

        IReadOnlyKey? primaryKey = entityType.FindPrimaryKey();
        Func<object, string> getEntityId = primaryKey is null
            ? static _ => string.Empty
            : PropertyMetadataCache.CreateKeyAccessor(primaryKey);

        return new EntityDescriptor(entityType, options, properties, getEntityId, null);
    }
}

/// <summary>
/// Builds compiled property accessors.
/// </summary>
public static class PropertyMetadataCache
{
    private static readonly ConcurrentDictionary<IProperty, PropertyDescriptor> PropertyCache = new();

    internal static PropertyDescriptor CreateDescriptor(IProperty property)
    {
        return PropertyCache.GetOrAdd(property, static p =>
        {
            Func<object, object?> getCurrent = entity =>
            {
                if (entity is not IPropertyAccessor accessor)
                {
                    return p.PropertyInfo?.GetValue(entity);
                }

                return accessor.GetProperty(p.Name);
            };

            return new PropertyDescriptor(p, getCurrent, null);
        });
    }

    internal static Func<object, string> CreateKeyAccessor(IReadOnlyKey key)
    {
        IReadOnlyProperty property = key.Properties[0];
        return entity =>
        {
            object? value = property.PropertyInfo?.GetValue(entity) ?? (entity as IPropertyAccessor)?.GetProperty(property.Name);
            return value?.ToString() ?? string.Empty;
        };
    }
}

internal interface IPropertyAccessor
{
    object? GetProperty(string name);
}
