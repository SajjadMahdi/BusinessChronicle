using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Validation;

namespace BusinessChronicle.Builders;

/// <summary>
/// Builds validated <see cref="CommitMessage"/> instances.
/// </summary>
public sealed class CommitMessageBuilder
{
    private string _text = string.Empty;
    private string? _shortDescription;

    /// <summary>
    /// Sets the commit message text.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>The builder instance.</returns>
    public CommitMessageBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    /// <summary>
    /// Sets the short description.
    /// </summary>
    /// <param name="shortDescription">The short description.</param>
    /// <returns>The builder instance.</returns>
    public CommitMessageBuilder WithShortDescription(string? shortDescription)
    {
        _shortDescription = shortDescription;
        return this;
    }

    /// <summary>
    /// Builds a validated commit message.
    /// </summary>
    /// <returns>A result containing the commit message or validation failure.</returns>
    public ChronicleResult<CommitMessage> Build()
    {
        CommitMessage message = new()
        {
            Text = _text,
            ShortDescription = _shortDescription,
        };

        DomainValidationResult validation = ChronicleDomainValidator.ValidateCommitMessage(message);
        if (!validation.IsValid)
        {
            return ChronicleResults.Failure<CommitMessage>(new ChronicleError(ChronicleErrorCode.ValidationFailed, string.Join(' ', validation.Errors)));
        }

        return ChronicleResults.Success(message);
    }
}
