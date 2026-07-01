namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Describes the human-readable message associated with a commit.
/// </summary>
public sealed record CommitMessage
{
    /// <summary>
    /// Gets the full commit message text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets an optional short summary suitable for timelines and activity feeds.
    /// </summary>
    public string? ShortDescription { get; init; }
}
