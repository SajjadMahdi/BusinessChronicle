using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Internal;

namespace BusinessChronicle.Pipeline;

/// <summary>
/// Extensibility point for commit pipeline stages.
/// </summary>
public interface ICommitPipelineStage
{
    /// <summary>
    /// Gets the stage name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the pipeline stage.
    /// </summary>
    ValueTask<ChronicleResult> ExecuteAsync(
        CommitPipelineState state,
        CancellationToken cancellationToken);
}
