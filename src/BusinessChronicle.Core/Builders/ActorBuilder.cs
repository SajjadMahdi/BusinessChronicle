using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="Actor"/> instances.
/// </summary>
public sealed class ActorBuilder
{
    private string _id = string.Empty;
    private ActorType _type;
    private string? _displayName;
    private FrozenDictionary<string, string>? _claims;

    /// <summary>
    /// Sets the actor identifier.
    /// </summary>
    /// <param name="id">The actor identifier.</param>
    /// <returns>The builder instance.</returns>
    public ActorBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the actor type.
    /// </summary>
    /// <param name="type">The actor type.</param>
    /// <returns>The builder instance.</returns>
    public ActorBuilder WithType(ActorType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the display name.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    /// <returns>The builder instance.</returns>
    public ActorBuilder WithDisplayName(string? displayName)
    {
        _displayName = displayName;
        return this;
    }

    /// <summary>
    /// Sets actor claims.
    /// </summary>
    /// <param name="claims">The actor claims.</param>
    /// <returns>The builder instance.</returns>
    public ActorBuilder WithClaims(FrozenDictionary<string, string>? claims)
    {
        _claims = claims;
        return this;
    }

    /// <summary>
    /// Builds a validated actor.
    /// </summary>
    /// <returns>A result containing the actor or validation failure.</returns>
    public ChronicleResult<Actor> Build()
    {
        Actor actor = new()
        {
            Id = _id,
            Type = _type,
            DisplayName = _displayName,
            Claims = _claims,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateActor(actor);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<Actor>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(actor);
    }
}
