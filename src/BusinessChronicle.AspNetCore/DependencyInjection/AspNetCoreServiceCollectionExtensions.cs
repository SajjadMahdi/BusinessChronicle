using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.AspNetCore.Http;
using BusinessChronicle.AspNetCore.Options;
using BusinessChronicle.AspNetCore.Services;
using BusinessChronicle.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BusinessChronicle.AspNetCore.DependencyInjection;

/// <summary>
/// ASP.NET Core dependency injection extensions for BusinessChronicle.
/// </summary>
public static class AspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds BusinessChronicle core and ASP.NET Core integration services.
    /// </summary>
    public static IServiceCollection AddBusinessChronicle(
        this IServiceCollection services,
        Action<Abstractions.Options.ChronicleOptions>? configure = null,
        Action<BusinessChronicleAspNetCoreOptions>? configureAspNetCore = null)
    {
        BusinessChronicle.DependencyInjection.ServiceCollectionExtensions.AddBusinessChronicle(services, configure);
        services.AddHttpContextAccessor();
        services.AddOptions<BusinessChronicleAspNetCoreOptions>().Configure(configureAspNetCore ?? (_ => { }));
        services.TryAddScoped<IActorResolver, HttpContextActorResolver>();
        services.TryAddSingleton<HttpCorrelationIdAccessor>();
        services.TryAddScoped<IChronicleApiService, ChronicleApiService>();
        services.AddAuthorizationBuilder()
            .AddPolicy(ChronicleAuthorizationPolicies.Read, static policy => policy.RequireAuthenticatedUser())
            .AddPolicy(ChronicleAuthorizationPolicies.Write, static policy => policy.RequireAuthenticatedUser());

        return services;
    }
}

/// <summary>
/// Authorization policy names for BusinessChronicle endpoints.
/// </summary>
public static class ChronicleAuthorizationPolicies
{
    /// <summary>
    /// Read-only chronicle access.
    /// </summary>
    public const string Read = "BusinessChronicle.Read";

    /// <summary>
    /// Write and rollback chronicle access.
    /// </summary>
    public const string Write = "BusinessChronicle.Write";
}
