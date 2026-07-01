using BusinessChronicle.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BusinessChronicle.EntityFrameworkCore.Metadata;

/// <summary>
/// Entity discovery and filtering helpers.
/// </summary>
public static class EntityDiscovery
{
    /// <summary>
    /// Gets entity options from EF metadata annotations.
    /// </summary>
    public static EntityChronicleOptions? GetEntityOptions(IReadOnlyEntityType entityType) =>
        entityType.FindAnnotation(EntityChronicleOptions.AnnotationName)?.Value as EntityChronicleOptions;

    /// <summary>
    /// Determines whether an entity type is enabled for chronicle capture.
    /// </summary>
    public static bool IsEnabled(IReadOnlyEntityType entityType) =>
        GetEntityOptions(entityType) is { Enabled: true };

    /// <summary>
    /// Applies entity options annotation.
    /// </summary>
    internal static void SetEntityOptions(IMutableEntityType entityType, EntityChronicleOptions options) =>
        entityType.SetAnnotation(EntityChronicleOptions.AnnotationName, options);
}

/// <summary>
/// Filters entity entries during change capture.
/// </summary>
public static class EntityFilter
{
    /// <summary>
    /// Determines whether an entity entry should be captured.
    /// </summary>
    public static bool ShouldCapture(EntityDescriptor descriptor, EntityState state) =>
        descriptor.Options.Enabled && state is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    /// <summary>
    /// Determines whether an owned entity entry should be captured.
    /// </summary>
    public static bool ShouldCaptureOwned(EntityDescriptor descriptor, bool captureOwnedEntities) =>
        captureOwnedEntities && descriptor.Options.Enabled;
}

/// <summary>
/// Filters properties during change capture.
/// </summary>
public static class PropertyFilter
{
    /// <summary>
    /// Determines whether a property should be captured.
    /// </summary>
    public static bool ShouldCapture(string propertyName, EntityChronicleOptions options) =>
        !options.IgnoredProperties.Contains(propertyName);
}

/// <summary>
/// Compares original and current property values.
/// </summary>
public static class PropertyComparer
{
    /// <summary>
    /// Determines whether values differ.
    /// </summary>
    public static bool HasChanged(object? original, object? current) =>
        !Equals(Normalize(original), Normalize(current));

    private static object? Normalize(object? value) => value switch
    {
        null => null,
        byte[] bytes => Convert.ToBase64String(bytes),
        _ => value,
    };
}
