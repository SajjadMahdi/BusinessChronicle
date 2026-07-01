namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Immutable inputs available while constructing a revision.
/// </summary>
public interface IRevisionContext
{
    /// <summary>
    /// Gets the entity the revision applies to.
    /// </summary>
    Models.EntityReference Entity { get; }

    /// <summary>
    /// Gets the revision classification.
    /// </summary>
    Enums.RevisionType Type { get; }

    /// <summary>
    /// Gets the owning commit identifier.
    /// </summary>
    Identifiers.CommitId CommitId { get; }

    /// <summary>
    /// Gets the parent revision identifier when extending a lineage.
    /// </summary>
    Identifiers.RevisionId? ParentRevisionId { get; }

    /// <summary>
    /// Gets the structural changes captured by the revision.
    /// </summary>
    IReadOnlyList<Models.ChangeDescriptor> Changes { get; }

    /// <summary>
    /// Gets revision metadata.
    /// </summary>
    Models.ChronicleMetadata Metadata { get; }

    /// <summary>
    /// Gets the version pointer assigned to the revision when pre-computed.
    /// </summary>
    Models.VersionPointer? Version { get; }
}
