namespace BusinessChronicle.Abstractions.Results;

/// <summary>
/// Describes an expected failure without throwing an exception.
/// </summary>
/// <param name="Code">The stable error code.</param>
/// <param name="Message">A human-readable summary of the failure.</param>
/// <param name="Detail">Optional diagnostic detail for logs or telemetry.</param>
public readonly record struct ChronicleError(
    Enums.ChronicleErrorCode Code,
    string Message,
    string? Detail = null);
