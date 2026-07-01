using BenchmarkDotNet.Attributes;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Engine;
using BusinessChronicle.Testing;
using BusinessChronicle.Testing.Assertions;

namespace BusinessChronicle.Benchmarks;

[MemoryDiagnoser]
public class SaveBenchmarks
{
    private ChronicleTestContext _context = null!;
    private EntityReference _entity = null!;

    [GlobalSetup]
    public void Setup()
    {
        _context = ChronicleTestContext.Create();
        _entity = new EntityReference { Id = new EntityId("order-1"), EntityType = "Order" };
    }

    [GlobalCleanup]
    public async ValueTask Cleanup() => await _context.DisposeAsync();

    [Benchmark]
    public async Task SaveRevision()
    {
        var sessionResult = await _context.CreateSessionAsync(_entity);
        await using var session = ChronicleAssertions.AssertSuccess(sessionResult);
        if (session is ChronicleSession concrete)
        {
            ChronicleAssertions.AssertSuccess(await concrete.SaveRevisionAsync(
                _entity,
                RevisionType.Update,
                [new ChangeDescriptor { Path = "Status", Kind = ChangeKind.Modified }],
                null,
                null,
                CancellationToken.None));
            session.CommitContext.Message = new CommitMessage { Text = "Update status" };
            ChronicleAssertions.AssertSuccess(await session.CommitAsync());
        }
    }
}

[MemoryDiagnoser]
public class HistoryBenchmarks
{
    private ChronicleTestContext _context = null!;
    private EntityReference _entity = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _context = ChronicleTestContext.Create();
        _entity = new EntityReference { Id = new EntityId("order-1"), EntityType = "Order" };
        var sessionResult = await _context.CreateSessionAsync(_entity);
        await using var session = ChronicleAssertions.AssertSuccess(sessionResult);
        if (session is ChronicleSession concrete)
        {
            ChronicleAssertions.AssertSuccess(await concrete.SaveRevisionAsync(
                _entity,
                RevisionType.Create,
                [],
                null,
                null,
                CancellationToken.None));
            session.CommitContext.Message = new CommitMessage { Text = "Seed history" };
            ChronicleAssertions.AssertSuccess(await session.CommitAsync());
        }
    }

    [GlobalCleanup]
    public async ValueTask Cleanup() => await _context.DisposeAsync();

    [Benchmark]
    public async Task ReadHistory()
    {
        _ = await _context.Store.Query.GetEntriesAsync(_entity, new Abstractions.Options.TimelineQueryOptions(), CancellationToken.None);
    }
}

public class CompareBenchmarks
{
    private ChronicleTestContext _context = null!;
    private EntityReference _entity = null!;

    [GlobalSetup]
    public void Setup()
    {
        _context = ChronicleTestContext.Create();
        _entity = new EntityReference { Id = new EntityId("order-1"), EntityType = "Order" };
    }

    [GlobalCleanup]
    public async ValueTask Cleanup() => await _context.DisposeAsync();

    [Benchmark]
    public async Task CompareRevisions()
    {
        _ = await _context.Store.RevisionReader.ListAsync(_entity, new Abstractions.Options.RevisionListOptions(), CancellationToken.None);
    }
}

public class RollbackBenchmarks
{
    [Benchmark]
    public static void RollbackOptionsCheck()
    {
        Abstractions.Options.ChronicleOptions options = new() { Entity = { AllowRollback = true } };
        _ = options.Entity.AllowRollback;
    }
}

public class SerializationBenchmarks
{
    private readonly Engine.DefaultChronicleSerializer _serializer = new();

    [Benchmark]
    public async Task SerializeMetadata()
    {
        _ = await _serializer.SerializeAsync(new Dictionary<string, string> { ["k"] = "v" }, CancellationToken.None);
    }
}

public class MemoryBenchmarks
{
    [Benchmark]
    public static void AllocateEntityReference()
    {
        _ = new EntityReference { Id = new EntityId(Guid.NewGuid().ToString("N")), EntityType = "Order" };
    }
}
