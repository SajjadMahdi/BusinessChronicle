using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Blazor;

/// <summary>
/// Blazor integration extensions.
/// </summary>
public static class BusinessChronicleBlazorExtensions
{
    /// <summary>
    /// Registers BusinessChronicle Blazor component services.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleBlazor(this IServiceCollection services) => services;
}
