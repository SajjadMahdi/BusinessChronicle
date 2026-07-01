namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Aggregate access point for chronicle storage operations.
/// </summary>
public interface IChronicleStore
{
    /// <summary>
    /// Gets the revision reader.
    /// </summary>
    IRevisionReader RevisionReader { get; }

    /// <summary>
    /// Gets the revision writer.
    /// </summary>
    IRevisionWriter RevisionWriter { get; }

    /// <summary>
    /// Gets the chronicle query surface.
    /// </summary>
    IChronicleQuery Query { get; }

    /// <summary>
    /// Gets the version graph navigator.
    /// </summary>
    IVersionGraph VersionGraph { get; }
}
