using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Internal;

internal sealed class RevisionContext : IRevisionContext
{
    public RevisionContext(
        EntityReference entity,
        RevisionType type,
        CommitId commitId,
        RevisionId? parentRevisionId,
        IReadOnlyList<ChangeDescriptor> changes,
        ChronicleMetadata metadata,
        VersionPointer? version)
    {
        Entity = entity;
        Type = type;
        CommitId = commitId;
        ParentRevisionId = parentRevisionId;
        Changes = changes;
        Metadata = metadata;
        Version = version;
    }

    public EntityReference Entity { get; }

    public RevisionType Type { get; }

    public CommitId CommitId { get; }

    public RevisionId? ParentRevisionId { get; }

    public IReadOnlyList<ChangeDescriptor> Changes { get; }

    public ChronicleMetadata Metadata { get; }

    public VersionPointer? Version { get; }
}
