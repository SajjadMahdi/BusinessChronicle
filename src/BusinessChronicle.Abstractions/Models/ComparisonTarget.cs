namespace BusinessChronicle.Abstractions.Models;

/// <summary>
/// Identifies the source and target of a revision comparison operation.
/// </summary>
public sealed record ComparisonTarget
{
    /// <summary>
    /// Gets the entity being compared.
    /// </summary>
    public required EntityReference Entity { get; init; }

    /// <summary>
    /// Gets the source revision identifier when comparing by revision.
    /// </summary>
    public Identifiers.RevisionId? SourceRevisionId { get; init; }

    /// <summary>
    /// Gets the target revision identifier when comparing by revision.
    /// </summary>
    public Identifiers.RevisionId? TargetRevisionId { get; init; }

    /// <summary>
    /// Gets the source version pointer when comparing by version number or label.
    /// </summary>
    public VersionPointer? SourceVersion { get; init; }

    /// <summary>
    /// Gets the target version pointer when comparing by version number or label.
    /// </summary>
    public VersionPointer? TargetVersion { get; init; }
}
