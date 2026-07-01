using BenchmarkDotNet.Attributes;
using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using BusinessChronicle.EntityFrameworkCore.Sqlite;
using BusinessChronicle.EntityFrameworkCore.Storage;
using BusinessChronicle.Testing.Assertions;
using BusinessChronicle.Testing.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Benchmarks;

[MemoryDiagnoser]
public class EfIntegrationBenchmarks : IDisposable
{
    private ServiceProvider _provider = null!;
    private EntityReference _entity = null!;
    private IChronicleStore _store = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        ServiceCollection services = new();
        services.AddDbContext<BenchmarkChronicleDbContext>(static options =>
            options.UseSqlite("Data Source=:memory:"));
        services.AddSingleton<FakeChronicleClock>();
        services.AddSingleton<IChronicleClock>(static sp => sp.GetRequiredService<FakeChronicleClock>());
        services.AddSingleton<FakeActorResolver>();
        services.AddSingleton<IActorResolver>(static sp => sp.GetRequiredService<FakeActorResolver>());
        services.AddScoped<IChronicleStore>(static sp =>
            new EfChronicleStore(sp.GetRequiredService<BenchmarkChronicleDbContext>()));

        _provider = services.BuildServiceProvider();
        _store = _provider.GetRequiredService<IChronicleStore>();
        _entity = new EntityReference { Id = new EntityId("order-ef-1"), EntityType = "Order" };

        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        BenchmarkChronicleDbContext context = scope.ServiceProvider.GetRequiredService<BenchmarkChronicleDbContext>();
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        CommitId commitId = new(Guid.NewGuid().ToString("N"));
        BcCommitEntity commit = new()
        {
            Id = commitId.Value,
            Message = "Seed EF benchmark",
            AuthorId = "benchmark",
            AuthorType = 0,
            CommittedAt = DateTimeOffset.UtcNow,
        };
        context.BcCommits.Add(commit);
        await context.SaveChangesAsync();

        Revision revision = new()
        {
            Id = new RevisionId(Guid.NewGuid().ToString("N")),
            CommitId = commitId,
            Entity = _entity,
            Type = RevisionType.Create,
            State = RevisionState.Active,
            Version = new VersionPointer { Number = 1 },
            CreatedAt = DateTimeOffset.UtcNow,
        };

        ChronicleAssertions.AssertSuccess(await _store.RevisionWriter.WriteAsync(revision, CancellationToken.None));
    }

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    [Benchmark]
    public async Task QueryHistoryViaEf()
    {
        _ = await _store.Query.GetEntriesAsync(_entity, new TimelineQueryOptions { MaxResults = 50 }, CancellationToken.None);
    }

    [Benchmark]
    public async Task ReadHeadViaEf()
    {
        _ = await _store.VersionGraph.GetHeadAsync(_entity, CancellationToken.None);
    }

    public void Dispose() => _provider.Dispose();
}

internal sealed class BenchmarkChronicleDbContext(DbContextOptions<BenchmarkChronicleDbContext> options) : DbContext(options)
{
    public DbSet<BcCommitEntity> BcCommits => Set<BcCommitEntity>();
    public DbSet<BcRevisionEntity> BcRevisions => Set<BcRevisionEntity>();
    public DbSet<BcDeltaEntity> BcDeltas => Set<BcDeltaEntity>();
    public DbSet<BcSnapshotEntity> BcSnapshots => Set<BcSnapshotEntity>();
    public DbSet<BcMetadataEntity> BcMetadata => Set<BcMetadataEntity>();
    public DbSet<BcTagEntity> BcTags => Set<BcTagEntity>();
    public DbSet<BcCommentEntity> BcComments => Set<BcCommentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ConfigureBusinessChronicleSqlite();
}
