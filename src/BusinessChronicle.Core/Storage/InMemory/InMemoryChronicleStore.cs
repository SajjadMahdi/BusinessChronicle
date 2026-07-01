using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Internal;

namespace BusinessChronicle.Storage.InMemory;

/// <summary>
/// Thread-safe in-memory chronicle store.
/// </summary>
public sealed class InMemoryChronicleStore : IChronicleStore, IDisposable
{
    private readonly InMemoryChronicleState _state = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryChronicleStore"/> class.
    /// </summary>
    public InMemoryChronicleStore()
    {
        RevisionReader = new RevisionReader(_state);
        RevisionWriter = new RevisionWriter(_state);
        Query = new ChronicleQuery(_state);
        VersionGraph = new VersionGraph(_state);
    }

    internal InMemoryChronicleState State => _state;

    /// <inheritdoc />
    public IRevisionReader RevisionReader { get; }

    /// <inheritdoc />
    public IRevisionWriter RevisionWriter { get; }

    /// <inheritdoc />
    public IChronicleQuery Query { get; }

    /// <inheritdoc />
    public IVersionGraph VersionGraph { get; }

    /// <inheritdoc />
    public void Dispose() => _state.Dispose();
}
