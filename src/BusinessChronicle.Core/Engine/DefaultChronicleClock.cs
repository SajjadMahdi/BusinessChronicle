using BusinessChronicle.Abstractions.Contracts;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default <see cref="IChronicleClock"/> implementation backed by <see cref="TimeProvider"/>.
/// </summary>
public sealed class DefaultChronicleClock(TimeProvider timeProvider) : IChronicleClock
{
    /// <inheritdoc />
    public TimeProvider TimeProvider { get; } = timeProvider;

    /// <inheritdoc />
    public DateTimeOffset GetUtcNow() => TimeProvider.GetUtcNow();
}
