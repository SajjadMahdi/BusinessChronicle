using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Extensions;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using BusinessChronicle.EntityFrameworkCore.Providers;
using Microsoft.EntityFrameworkCore;

namespace BusinessChronicle.EntityFrameworkCore.Sqlite;

/// <summary>
/// SQLite provider optimizations for BusinessChronicle.
/// </summary>
public sealed class SqliteBusinessChronicleProvider : IBusinessChronicleDatabaseProvider
{
    /// <inheritdoc />
    public string Name => "Sqlite";

    /// <inheritdoc />
    public void ConfigureModel(ModelBuilder modelBuilder, BusinessChronicleEfOptions options)
    {
        modelBuilder.Entity<BcSnapshotEntity>()
            .Property(static e => e.Payload)
            .HasColumnType("BLOB");

        modelBuilder.Entity<BcRevisionEntity>()
            .HasIndex(static e => new { e.EntityType, e.EntityId, e.VersionNumber })
            .IsUnique();
    }
}

/// <summary>
/// SQLite registration extensions.
/// </summary>
public static class SqliteBusinessChronicleExtensions
{
    /// <summary>
    /// Configures BusinessChronicle defaults with SQLite optimizations.
    /// </summary>
    public static ModelBuilder ConfigureBusinessChronicleSqlite(
        this ModelBuilder modelBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        BusinessChronicleModelBuilderExtensions.ConfigureBusinessChronicleDefaults(
            modelBuilder,
            configure,
            new SqliteBusinessChronicleProvider());

    /// <summary>
    /// Uses SQLite BusinessChronicle provider options.
    /// </summary>
    public static DbContextOptionsBuilder UseBusinessChronicleSqlite(
        this DbContextOptionsBuilder optionsBuilder,
        Action<BusinessChronicleOptionsBuilder>? configure = null) =>
        optionsBuilder.UseBusinessChronicle(configure);
}
