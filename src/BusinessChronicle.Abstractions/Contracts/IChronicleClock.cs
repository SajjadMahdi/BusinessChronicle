namespace BusinessChronicle.Abstractions.Contracts;

/// <summary>
/// Provides UTC timestamps for chronicle operations using <see cref="TimeProvider"/>.
/// </summary>
public interface IChronicleClock
{
    /// <summary>
    /// Gets the time provider used by chronicle components.
    /// </summary>
    TimeProvider TimeProvider { get; }

    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    DateTimeOffset GetUtcNow();
}
