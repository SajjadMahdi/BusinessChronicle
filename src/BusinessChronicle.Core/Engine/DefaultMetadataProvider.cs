using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default metadata provider that merges correlation identifiers into commit metadata.
/// </summary>
public sealed class DefaultMetadataProvider : IChronicleMetadataProvider
{
    /// <inheritdoc />
    public ValueTask<ChronicleMetadata> GetMetadataAsync(
        EntityReference entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = entity;
        return ValueTask.FromResult(ChronicleMetadata.Empty);
    }

    /// <inheritdoc />
    public ValueTask<ChronicleResult<ChronicleMetadata>> EnrichAsync(
        ChronicleMetadata metadata,
        ICommitContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Correlation.Count == 0)
        {
            return ValueTask.FromResult(ChronicleResults.Success(metadata));
        }

        Dictionary<string, string> merged = new(metadata.Values, StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in context.Correlation)
        {
            merged[pair.Key] = pair.Value;
        }

        return ValueTask.FromResult(ChronicleResults.Success(new ChronicleMetadata(merged.ToFrozenDictionary(StringComparer.Ordinal))));
    }
}
