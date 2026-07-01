using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="ComparisonTarget"/> instances.
/// </summary>
public sealed class ComparisonTargetBuilder
{
    private EntityReference _entity = null!;
    private RevisionId? _sourceRevisionId;
    private RevisionId? _targetRevisionId;
    private VersionPointer? _sourceVersion;
    private VersionPointer? _targetVersion;

    /// <summary>
    /// Sets the entity reference.
    /// </summary>
    /// <param name="entity">The entity reference.</param>
    /// <returns>The builder instance.</returns>
    public ComparisonTargetBuilder WithEntity(EntityReference entity)
    {
        _entity = entity;
        return this;
    }

    /// <summary>
    /// Sets revision-based comparison endpoints.
    /// </summary>
    /// <param name="sourceRevisionId">The source revision identifier.</param>
    /// <param name="targetRevisionId">The target revision identifier.</param>
    /// <returns>The builder instance.</returns>
    public ComparisonTargetBuilder WithRevisions(RevisionId sourceRevisionId, RevisionId targetRevisionId)
    {
        _sourceRevisionId = sourceRevisionId;
        _targetRevisionId = targetRevisionId;
        _sourceVersion = null;
        _targetVersion = null;
        return this;
    }

    /// <summary>
    /// Sets version-pointer-based comparison endpoints.
    /// </summary>
    /// <param name="sourceVersion">The source version pointer.</param>
    /// <param name="targetVersion">The target version pointer.</param>
    /// <returns>The builder instance.</returns>
    public ComparisonTargetBuilder WithVersions(VersionPointer sourceVersion, VersionPointer targetVersion)
    {
        _sourceVersion = sourceVersion;
        _targetVersion = targetVersion;
        _sourceRevisionId = null;
        _targetRevisionId = null;
        return this;
    }

    /// <summary>
    /// Builds a validated comparison target.
    /// </summary>
    /// <returns>A result containing the comparison target or validation failure.</returns>
    public ChronicleResult<ComparisonTarget> Build()
    {
        ComparisonTarget target = new()
        {
            Entity = _entity,
            SourceRevisionId = _sourceRevisionId,
            TargetRevisionId = _targetRevisionId,
            SourceVersion = _sourceVersion,
            TargetVersion = _targetVersion,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateComparisonTarget(target);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<ComparisonTarget>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(target);
    }
}
