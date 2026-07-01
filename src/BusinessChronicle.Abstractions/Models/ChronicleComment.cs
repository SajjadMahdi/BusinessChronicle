namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// A human annotation attached to a chronicle entry.
/// </summary>
public sealed record ChronicleComment
{
    /// <summary>
    /// Gets the comment identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the comment text.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the author of the comment.
    /// </summary>
    public required Actor Author { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the comment was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
