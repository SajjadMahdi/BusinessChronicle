namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Outcome of validating domain invariants without throwing.
/// </summary>
public readonly record struct DomainValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationResult"/> struct.
    /// </summary>
    /// <param name="errors">Validation error messages; empty when valid.</param>
    public DomainValidationResult(IReadOnlyList<string> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the validation error messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the validated value satisfies domain invariants.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static DomainValidationResult Valid { get; } = new([]);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">One or more validation error messages.</param>
    /// <returns>A failed validation result.</returns>
    public static DomainValidationResult Invalid(params ReadOnlySpan<string> errors)
    {
        return new DomainValidationResult(errors.ToArray());
    }
}
