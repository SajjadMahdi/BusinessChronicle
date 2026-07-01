using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Internal;

/// <summary>
/// Mutable state shared across commit pipeline stages.
/// </summary>
public sealed class CommitPipelineState
{
    /// <summary>
    /// Gets the commit context.
    /// </summary>
    public required ICommitContext Context { get; init; }

    /// <summary>
    /// Gets or sets the resolved actor.
    /// </summary>
    public Actor? ResolvedActor { get; set; }

    /// <summary>
    /// Gets or sets enriched metadata.
    /// </summary>
    public ChronicleMetadata EnrichedMetadata { get; set; } = ChronicleMetadata.Empty;

    internal CommitContext InternalContext => (CommitContext)Context;
}
