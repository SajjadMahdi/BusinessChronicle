using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Domain;

namespace BusinessChronicle.Validation;

/// <summary>
/// Validates domain model invariants without throwing.
/// </summary>
public static class ChronicleDomainValidator
{
    /// <summary>
    /// Validates an <see cref="EntityReference"/>.
    /// </summary>
    /// <param name="reference">The entity reference.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateEntityReference(EntityReference reference)
    {
        List<string> errors = [];

        if (reference.Id.IsEmpty)
        {
            errors.Add("Entity identifier must not be empty.");
        }

        if (reference.EntityType is { Length: > 0 } entityType &&
            entityType.Length > ChronicleDomainLimits.MaxEntityTypeLength)
        {
            errors.Add($"Entity type must not exceed {ChronicleDomainLimits.MaxEntityTypeLength} characters.");
        }

        if (reference.DisplayName is { Length: > 0 } displayName &&
            displayName.Length > ChronicleDomainLimits.MaxDisplayNameLength)
        {
            errors.Add($"Display name must not exceed {ChronicleDomainLimits.MaxDisplayNameLength} characters.");
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates an <see cref="Actor"/>.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateActor(Actor actor)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(actor.Id))
        {
            errors.Add("Actor identifier must not be empty.");
        }
        else if (actor.Id.Length > ChronicleDomainLimits.MaxActorIdLength)
        {
            errors.Add($"Actor identifier must not exceed {ChronicleDomainLimits.MaxActorIdLength} characters.");
        }

        if (actor.DisplayName is { Length: > 0 } displayName &&
            displayName.Length > ChronicleDomainLimits.MaxDisplayNameLength)
        {
            errors.Add($"Actor display name must not exceed {ChronicleDomainLimits.MaxDisplayNameLength} characters.");
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="CommitMessage"/>.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateCommitMessage(CommitMessage message)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(message.Text))
        {
            errors.Add("Commit message text must not be empty.");
        }
        else if (message.Text.Length > ChronicleDomainLimits.MaxCommitMessageLength)
        {
            errors.Add($"Commit message must not exceed {ChronicleDomainLimits.MaxCommitMessageLength} characters.");
        }

        if (message.ShortDescription is { Length: > 0 } shortDescription &&
            shortDescription.Length > ChronicleDomainLimits.MaxShortDescriptionLength)
        {
            errors.Add($"Short description must not exceed {ChronicleDomainLimits.MaxShortDescriptionLength} characters.");
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="Commit"/>.
    /// </summary>
    /// <param name="commit">The commit.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateCommit(Commit commit)
    {
        List<string> errors = [];

        if (commit.Id.IsEmpty)
        {
            errors.Add("Commit identifier must not be empty.");
        }

        errors.AddRange(ValidateCommitMessage(commit.Message).Errors);
        errors.AddRange(ValidateActor(commit.Author).Errors);

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="Revision"/>.
    /// </summary>
    /// <param name="revision">The revision.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateRevision(Revision revision)
    {
        List<string> errors = [];

        if (revision.Id.IsEmpty)
        {
            errors.Add("Revision identifier must not be empty.");
        }

        if (revision.CommitId.IsEmpty)
        {
            errors.Add("Commit identifier must not be empty.");
        }

        errors.AddRange(ValidateEntityReference(revision.Entity).Errors);

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="Snapshot"/>.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateSnapshot(Snapshot snapshot)
    {
        List<string> errors = [];

        if (snapshot.RevisionId.IsEmpty)
        {
            errors.Add("Revision identifier must not be empty.");
        }

        errors.AddRange(ValidateEntityReference(snapshot.Entity).Errors);

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="ComparisonTarget"/>.
    /// </summary>
    /// <param name="target">The comparison target.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateComparisonTarget(ComparisonTarget target)
    {
        List<string> errors = [];

        errors.AddRange(ValidateEntityReference(target.Entity).Errors);

        var hasRevisionPair = target.SourceRevisionId is not null || target.TargetRevisionId is not null;
        var hasVersionPair = target.SourceVersion is not null || target.TargetVersion is not null;

        if (hasRevisionPair && hasVersionPair)
        {
            errors.Add("Comparison target must specify either revision identifiers or version pointers, not both.");
        }

        if (target.SourceRevisionId is { } sourceRevision && sourceRevision.IsEmpty)
        {
            errors.Add("Source revision identifier must not be empty.");
        }

        if (target.TargetRevisionId is { } targetRevision && targetRevision.IsEmpty)
        {
            errors.Add("Target revision identifier must not be empty.");
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="ChronicleEntry"/>.
    /// </summary>
    /// <param name="entry">The chronicle entry.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateChronicleEntry(ChronicleEntry entry)
    {
        List<string> errors = [];

        if (entry.RevisionId.IsEmpty)
        {
            errors.Add("Revision identifier must not be empty.");
        }

        if (entry.CommitId.IsEmpty)
        {
            errors.Add("Commit identifier must not be empty.");
        }

        errors.AddRange(ValidateEntityReference(entry.Entity).Errors);
        errors.AddRange(ValidateActor(entry.Actor).Errors);
        errors.AddRange(ValidateCommitMessage(entry.Message).Errors);

        foreach (ChronicleTag tag in entry.Tags)
        {
            if (string.IsNullOrWhiteSpace(tag.Name))
            {
                errors.Add("Tag name must not be empty.");
            }
            else if (tag.Name.Length > ChronicleDomainLimits.MaxTagNameLength)
            {
                errors.Add($"Tag name must not exceed {ChronicleDomainLimits.MaxTagNameLength} characters.");
            }
        }

        foreach (ChronicleComment comment in entry.Comments)
        {
            if (string.IsNullOrWhiteSpace(comment.Id))
            {
                errors.Add("Comment identifier must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(comment.Text))
            {
                errors.Add("Comment text must not be empty.");
            }
            else if (comment.Text.Length > ChronicleDomainLimits.MaxCommentLength)
            {
                errors.Add($"Comment text must not exceed {ChronicleDomainLimits.MaxCommentLength} characters.");
            }

            errors.AddRange(ValidateActor(comment.Author).Errors);
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="ChangeDescriptor"/>.
    /// </summary>
    /// <param name="descriptor">The change descriptor.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidateChangeDescriptor(ChangeDescriptor descriptor)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(descriptor.Path))
        {
            errors.Add("Change path must not be empty.");
        }
        else if (descriptor.Path.Length > ChronicleDomainLimits.MaxChangePathLength)
        {
            errors.Add($"Change path must not exceed {ChronicleDomainLimits.MaxChangePathLength} characters.");
        }

        if (descriptor.PropertyChange is { } propertyChange)
        {
            errors.AddRange(ValidatePropertyChange(propertyChange).Errors);
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }

    /// <summary>
    /// Validates a <see cref="PropertyChange"/>.
    /// </summary>
    /// <param name="change">The property change.</param>
    /// <returns>A validation result.</returns>
    public static DomainValidationResult ValidatePropertyChange(PropertyChange change)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(change.PropertyName))
        {
            errors.Add("Property name must not be empty.");
        }
        else if (change.PropertyName.Length > ChronicleDomainLimits.MaxPropertyNameLength)
        {
            errors.Add($"Property name must not exceed {ChronicleDomainLimits.MaxPropertyNameLength} characters.");
        }

        return errors.Count == 0 ? DomainValidationResult.Valid : new DomainValidationResult(errors);
    }
}
