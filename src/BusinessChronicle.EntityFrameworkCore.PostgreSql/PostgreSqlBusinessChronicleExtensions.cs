using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Extensions;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using BusinessChronicle.EntityFrameworkCore.Providers;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BusinessChronicle.EntityFrameworkCore.PostgreSql;

/// <summary>
/// PostgreSQL provider optimizations for BusinessChronicle.
/// </summary>
public sealed class PostgreSqlBusinessChronicleProvider : IBusinessChronicleDatabaseProvider
{
    /// <inheritdoc />
    public string Name => "PostgreSQL";

    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        ConfigureJsonbColumns(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureTimestamps(modelBuilder);
    }

    private static void ConfigureJsonbColumns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcCommitEntity>().Property(static e => e.MetadataJson).HasColumnType("jsonb");
        modelBuilder.Entity<BcRevisionEntity>().Property(static e => e.MetadataJson).HasColumnType("jsonb");
        modelBuilder.Entity<BcSnapshotEntity>().Property(static e => e.MetadataJson).HasColumnType("jsonb");
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcRevisionEntity>().HasIndex(static e => new { e.EntityType, e.EntityId, e.CreatedAt });
        modelBuilder.Entity<BcMetadataEntity>().HasIndex(static e => e.Value).HasMethod("gin");
    }

    private static void ConfigureTimestamps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BcCommitEntity>().Property(static e => e.CommittedAt).HasColumnType("timestamptz");
        modelBuilder.Entity<BcRevisionEntity>().Property(static e => e.CreatedAt).HasColumnType("timestamptz");
        modelBuilder.Entity<BcSnapshotEntity>().Property(static e => e.CapturedAt).HasColumnType("timestamptz");
    }
}

/// <summary>
/// PostgreSQL registration extensions.
/// </summary>
public static class PostgreSqlBusinessChronicleExtensions
{
    /// <summary>
    /// Configures BusinessChronicle defaults with PostgreSQL optimizations.
    /// </summary>
    public static ModelBuilder ConfigureBusinessChroniclePostgreSql(
        this ModelBuilder modelBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        BusinessChronicleModelBuilderExtensions.ConfigureBusinessChronicleDefaults(
            modelBuilder,
            configure,
            new PostgreSqlBusinessChronicleProvider());

    /// <summary>
    /// Uses PostgreSQL BusinessChronicle provider options.
    /// </summary>
    public static DbContextOptionsBuilder UseBusinessChroniclePostgreSql(
        this DbContextOptionsBuilder optionsBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        optionsBuilder.UseBusinessChronicle(configure);
}
