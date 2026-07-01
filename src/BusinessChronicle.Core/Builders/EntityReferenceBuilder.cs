using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="EntityReference"/> instances.
/// </summary>
public sealed class EntityReferenceBuilder
{
    private EntityId _id;
    private string? _entityType;
    private string? _displayName;

    /// <summary>
    /// Sets the entity identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The builder instance.</returns>
    public EntityReferenceBuilder WithId(EntityId id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the entity type name.
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <returns>The builder instance.</returns>
    public EntityReferenceBuilder WithEntityType(string? entityType)
    {
        _entityType = entityType;
        return this;
    }

    /// <summary>
    /// Sets the display name.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    /// <returns>The builder instance.</returns>
    public EntityReferenceBuilder WithDisplayName(string? displayName)
    {
        _displayName = displayName;
        return this;
    }

    /// <summary>
    /// Builds a validated entity reference.
    /// </summary>
    /// <returns>A result containing the entity reference or validation failure.</returns>
    public ChronicleResult<EntityReference> Build()
    {
        EntityReference reference = new()
        {
            Id = _id,
            EntityType = _entityType,
            DisplayName = _displayName,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateEntityReference(reference);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<EntityReference>(CreateValidationError(validation));
        }

        return ChronicleResults.Success(reference);
    }

    private static ChronicleError CreateValidationError(DomainValidationResult validation) =>
        new(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors));
}
