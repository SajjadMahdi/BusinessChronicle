using BusinessChronicle.AspNetCore.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace BusinessChronicle.AspNetCore.OpenApi;

/// <summary>
/// OpenAPI and Scalar documentation extensions.
/// </summary>
public static class ChronicleOpenApiExtensions
{
    /// <summary>
    /// Adds BusinessChronicle OpenAPI document generation.
    /// </summary>
    public static IServiceCollection AddBusinessChronicleOpenApi(
        this IServiceCollection services,
        Action<BusinessChronicleAspNetCoreOptions>? configure = null)
    {
        services.AddOptions<BusinessChronicleAspNetCoreOptions>().Configure(configure ?? (_ => { }));
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                BusinessChronicleAspNetCoreOptions aspOptions = context.ApplicationServices
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<BusinessChronicleAspNetCoreOptions>>()
                    .Value;

                document.Info.Title = aspOptions.OpenApiTitle;
                document.Info.Version = aspOptions.OpenApiVersion;
                document.Info.Description = "BusinessChronicle audit, versioning, and timeline API.";
                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Maps Scalar API reference UI for BusinessChronicle (not Swagger UI).
    /// </summary>
    public static WebApplication MapBusinessChronicleScalar(this WebApplication app, string routePrefix = "/scalar")
    {
        app.MapOpenApi();
        app.MapScalarApiReference(routePrefix, options =>
        {
            options.WithTitle("BusinessChronicle API");
            options.WithTheme(ScalarTheme.BluePlanet);
        });

        return app;
    }
}
