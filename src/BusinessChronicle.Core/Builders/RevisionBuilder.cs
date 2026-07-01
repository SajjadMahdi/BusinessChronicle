using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="Revision"/> instances.
/// </summary>
public sealed class RevisionBuilder
{
    private RevisionId _id;
    private CommitId _commitId;
    private EntityReference _entity = null!;
    private RevisionType _type;
    private RevisionState _state;
    private RevisionId? _parentRevisionId;
    private VersionPointer? _version;
    private DateTimeOffset _createdAt;
    private ChronicleMetadata _metadata = ChronicleMetadata.Empty;

    /// <summary>
    /// Sets the revision identifier.
    /// </summary>
    /// <param name="id">The revision identifier.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithId(RevisionId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the owning commit identifier.
    /// </summary>
    /// <param name="commitId">The commit identifier.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithCommitId(CommitId commitId)
    {
        _commitId = commitId;
        return this;
    }

    /// <summary>
    /// Sets the entity reference.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithEntity(EntityReference entity)
    {
        _entity = entity;
        return this;
    }

    /// <summary>
    /// Sets the revision type.
    /// </summary>
    /// <param name="type">The revision type.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithType(RevisionType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the revision state.
    /// </summary>
    /// <param name="state">The revision state.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithState(RevisionState state)
    {
        _state = state;
        return this;
    }

    /// <summary>
    /// Sets the parent revision identifier.
    /// </summary>
    /// <param name="parentRevisionId">The parent revision identifier.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithParentRevisionId(RevisionId? parentRevisionId)
    {
        _parentRevisionId = parentRevisionId;
        return this;
    }

    /// <summary>
    /// Sets the version pointer.
    /// </summary>
    /// <param name="version">The version pointer.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithVersion(VersionPointer? version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the creation timestamp.
    /// </summary>
    /// <param name="createdAt">The UTC creation timestamp.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets revision metadata.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The builder instance.</returns>
    public RevisionBuilder WithMetadata(ChronicleMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Builds a validated revision.
    /// </summary>
    /// <returns>A result containing the revision or validation failure.</returns>
    public ChronicleResult<Revision> Build()
    {
        Revision revision = new()
        {
            Id = _id,
            CommitId = _commitId,
            Entity = _entity,
            Type = _type,
            State = _state,
            ParentRevisionId = _parentRevisionId,
            Version = _version,
            CreatedAt = _createdAt,
            Metadata = _metadata,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateRevision(revision);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<Revision>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(revision);
    }
}
