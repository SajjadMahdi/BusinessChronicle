using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Extensions;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using BusinessChronicle.EntityFrameworkCore.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessChronicle.EntityFrameworkCore.SqlServer;

/// <summary>
/// SQL Server provider optimizations for BusinessChronicle.
/// </summary>
public sealed class SqlServerBusinessChronicleProvider : IBusinessChronicleDatabaseProvider
{
    /// <inheritdoc />
    public string Name => "SqlServer";

    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        ConfigureJsonColumns(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureCompression(modelBuilder);
    }

    private static void ConfigureJsonColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcCommitEntity>().Property(static e => e.MetadataJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<BcRevisionEntity>().Property(static e => e.MetadataJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<BcSnapshotEntity>().Property(static e => e.MetadataJson).HasColumnType("nvarchar(max)");
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcRevisionEntity>()
            .HasIndex(static e => new { e.EntityType, e.EntityId, e.VersionNumber })
            .IsUnique();

        modelBuilder.Entity<BcSnapshotEntity>()
            .HasIndex(static e => e.RevisionId)
            .IncludeProperties(static e => new { e.EntityType, e.EntityId });
    }

    private static void ConfigureCompression(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcDeltaEntity>().HasAnnotation("SqlServer:UseRowCompression", true);
        modelBuilder.Entity<BcSnapshotEntity>().HasAnnotation("SqlServer:UseRowCompression", true);
    }
}

/// <summary>
/// SQL Server registration extensions.
/// </summary>
public static class SqlServerBusinessChronicleExtensions
{
    /// <summary>
    /// Configures BusinessChronicle defaults with SQL Server optimizations.
    /// </summary>
    public static ModelBuilder ConfigureBusinessChronicleSqlServer(
        this ModelBuilder modelBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        BusinessChronicleModelBuilderExtensions.ConfigureBusinessChronicleDefaults(
            modelBuilder,
            configure,
            new SqlServerBusinessChronicleProvider());

    /// <summary>
    /// Uses SQL Server BusinessChronicle provider options.
    /// </summary>
    public static DbContextOptionsBuilder UseBusinessChronicleSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        optionsBuilder.UseBusinessChronicle(configure);
}
