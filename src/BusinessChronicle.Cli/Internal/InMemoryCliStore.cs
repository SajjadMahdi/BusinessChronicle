using BusinessChronicle.DependencyInjection;
using BusinessChronicle.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Cli.Internal;

/// <summary>
/// Builds an in-memory service provider for CLI operations.
/// </summary>
internal static class InMemoryCliStore
{
    internal static ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddBusinessChronicle();
        services.AddBusinessChronicleTesting();
        return services.BuildServiceProvider();
    }
}
