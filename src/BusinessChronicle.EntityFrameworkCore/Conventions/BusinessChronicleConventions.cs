using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Metadata;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BusinessChronicle.EntityFrameworkCore.Conventions;

internal static class BusinessChronicleModelAnnotations
{
    internal const string Options = "BusinessChronicle:Options";
}

/// <summary>
/// Applies BusinessChronicle conventions during model building.
/// </summary>
public static class BusinessChronicleConventionExtensions
{
    /// <summary>
    /// Configures BusinessChronicle model conventions.
    /// </summary>
    public static ModelBuilder ConfigureBusinessChronicleConventions(
        this ModelBuilder modelBuilder,
        BusinessChronicleEfOptions options)
    {
        if (!options.EnableEntitiesByConvention)
        {
            return modelBuilder;
        }

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (IsChronicleEntity(entityType.ClrType) || EntityDiscovery.GetEntityOptions(entityType) is not null)
            {
                continue;
            }

            EntityDiscovery.SetEntityOptions(entityType, options.DefaultEntityOptions.Clone());
        }

        return modelBuilder;
    }

    private static bool IsChronicleEntity(Type clrType) =>
        clrType == typeof(BcCommitEntity) ||
        clrType == typeof(BcRevisionEntity) ||
        clrType == typeof(BcDeltaEntity) ||
        clrType == typeof(BcSnapshotEntity) ||
        clrType == typeof(BcMetadataEntity) ||
        clrType == typeof(BcTagEntity) ||
        clrType == typeof(BcCommentEntity);
}
