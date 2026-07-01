using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="Commit"/> instances.
/// </summary>
public sealed class CommitBuilder
{
    private CommitId _id;
    private CommitMessage _message = null!;
    private Actor _author = null!;
    private DateTimeOffset _committedAt;
    private CommitId? _parentCommitId;
    private List<RevisionReference> _revisions = [];
    private ChronicleMetadata _metadata = ChronicleMetadata.Empty;

    /// <summary>
    /// Sets the commit identifier.
    /// </summary>
    /// <param name="id">The commit identifier.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithId(CommitId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the commit message.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithMessage(CommitMessage message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Sets the author.
    /// </summary>
    /// <param name="author">The author.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithAuthor(Actor author)
    {
        _author = author;
        return this;
    }

    /// <summary>
    /// Sets the commit timestamp.
    /// </summary>
    /// <param name="committedAt">The UTC commit timestamp.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithCommittedAt(DateTimeOffset committedAt)
    {
        _committedAt = committedAt;
        return this;
    }

    /// <summary>
    /// Sets the parent commit identifier.
    /// </summary>
    /// <param name="parentCommitId">The parent commit identifier.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithParentCommitId(CommitId? parentCommitId)
    {
        _parentCommitId = parentCommitId;
        return this;
    }

    /// <summary>
    /// Adds a revision reference to the commit.
    /// </summary>
    /// <param name="revision">The revision reference.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder AddRevision(RevisionReference revision)
    {
        _revisions.Add(revision);
        return this;
    }

    /// <summary>
    /// Sets commit metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The builder instance.</returns>
    public CommitBuilder WithMetadata(ChronicleMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Builds a validated commit.
    /// </summary>
    /// <returns>A result containing the commit or validation failure.</returns>
    public ChronicleResult<Commit> Build()
    {
        Commit commit = new()
        {
            Id = _id,
            Message = _message,
            Author = _author,
            CommittedAt = _committedAt,
            ParentCommitId = _parentCommitId,
            Revisions = _revisions,
            Metadata = _metadata,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateCommit(commit);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<Commit>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(commit);
    }
}
