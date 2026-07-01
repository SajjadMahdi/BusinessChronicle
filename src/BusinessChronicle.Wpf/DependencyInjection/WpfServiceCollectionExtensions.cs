using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Wpf.DependencyInjection;

/// <summary>
/// WPF integration extensions.
/// </summary>
public static class WpfServiceCollectionExtensions
{
    /// <summary>
    /// Registers BusinessChronicle WPF services.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleWpf(this IServiceCollection services) => services;
}
