using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="ChronicleEntry"/> instances.
/// </summary>
public sealed class ChronicleEntryBuilder
{
    private RevisionId _revisionId;
    private CommitId _commitId;
    private EntityReference _entity = null!;
    private Actor _actor = null!;
    private DateTimeOffset _occurredAt;
    private CommitMessage _message = null!;
    private RevisionType _revisionType;
    private List<ChangeDescriptor> _changes = [];
    private ChronicleMetadata _metadata = ChronicleMetadata.Empty;
    private List<ChronicleTag> _tags = [];
    private List<ChronicleComment> _comments = [];

    /// <summary>
    /// Sets the revision identifier.
    /// </summary>
    /// <param name="revisionId">The revision identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithRevisionId(RevisionId revisionId)
    {
        _revisionId = revisionId;
        return this;
    }

    /// <summary>
    /// Sets the commit identifier.
    /// </summary>
    /// <param name="commitId">The commit identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithCommitId(CommitId commitId)
    {
        _commitId = commitId;
        return this;
    }

    /// <summary>
    /// Sets the entity reference.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithEntity(EntityReference entity)
    {
        _entity = entity;
        return this;
    }

    /// <summary>
    /// Sets the actor.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithActor(Actor actor)
    {
        _actor = actor;
        return this;
    }

    /// <summary>
    /// Sets the occurrence timestamp.
    /// </summary>
    /// <param name="occurredAt">The UTC occurrence timestamp.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithOccurredAt(DateTimeOffset occurredAt)
    {
        _occurredAt = occurredAt;
        return this;
    }

    /// <summary>
    /// Sets the commit message.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithMessage(CommitMessage message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Sets the revision type.
    /// </summary>
    /// <param name="revisionType">The revision type.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithRevisionType(RevisionType revisionType)
    {
        _revisionType = revisionType;
        return this;
    }

    /// <summary>
    /// Adds a change descriptor.
    /// </summary>
    /// <param name="change">The change descriptor.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder AddChange(ChangeDescriptor change)
    {
        _changes.Add(change);
        return this;
    }

    /// <summary>
    /// Sets entry metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder WithMetadata(ChronicleMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Adds a tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder AddTag(ChronicleTag tag)
    {
        _tags.Add(tag);
        return this;
    }

    /// <summary>
    /// Adds a comment.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <returns>The builder instance.</returns>
    public ChronicleEntryBuilder AddComment(ChronicleComment comment)
    {
        _comments.Add(comment);
        return this;
    }

    /// <summary>
    /// Builds a validated chronicle entry.
    /// </summary>
    /// <returns>A result containing the chronicle entry or validation failure.</returns>
    public ChronicleResult<ChronicleEntry> Build()
    {
        ChronicleEntry entry = new()
        {
            RevisionId = _revisionId,
            CommitId = _commitId,
            Entity = _entity,
            Actor = _actor,
            OccurredAt = _occurredAt,
            Message = _message,
            RevisionType = _revisionType,
            Changes = _changes,
            Metadata = _metadata,
            Tags = _tags,
            Comments = _comments,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateChronicleEntry(entry);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<ChronicleEntry>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(entry);
    }
}
