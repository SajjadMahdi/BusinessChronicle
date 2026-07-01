using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;

namespace BusinessChronicle.Internal;

internal sealed class CommitContext : ICommitContext
{
    private readonly List<RevisionReference> _stagedRevisions = [];

    internal CommitContext(CommitId commitId, EntityReference scopeEntity, FrozenDictionary<string, string>? correlation)
    {
        CommitId = commitId;
        ScopeEntity = scopeEntity;
        Correlation = correlation ?? FrozenDictionary<string, string>.Empty;
        Metadata = ChronicleMetadata.Empty;
        Message = new CommitMessage { Text = string.Empty };
    }

    internal CommitId CommitId { get; }

    internal EntityReference ScopeEntity { get; }

    public CommitMessage Message { get; set; }

    public Actor? Author { get; set; }

    public ChronicleMetadata Metadata { get; set; }

    public IReadOnlyList<RevisionReference> StagedRevisions => _stagedRevisions;

    public FrozenDictionary<string, string> Correlation { get; }

    public void StageRevision(RevisionReference revision) => _stagedRevisions.Add(revision);
}
