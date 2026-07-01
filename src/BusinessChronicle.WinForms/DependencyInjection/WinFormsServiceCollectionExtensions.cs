using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.WinForms.DependencyInjection;

/// <summary>
/// WinForms integration extensions.
/// </summary>
public static class WinFormsServiceCollectionExtensions
{
    /// <summary>
    /// Registers BusinessChronicle WinForms services.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleWinForms(this IServiceCollection services) => services;
}
