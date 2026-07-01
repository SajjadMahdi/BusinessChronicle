namespace BusinessChronicle.AspNetCore.Options;

/// <summary>
/// ASP.NET Core integration options for BusinessChronicle.
/// </summary>
public sealed class BusinessChronicleAspNetCoreOptions
{
    /// <summary>
    /// Gets or sets the API route prefix.
    /// </summary>
    public string RoutePrefix { get; set; } = "/api/chronicle";

    /// <summary>
    /// Gets or sets the authorization policy name applied to chronicle endpoints.
    /// </summary>
    public string? AuthorizationPolicy { get; set; }

    /// <summary>
    /// Gets or sets the correlation header name.
    /// </summary>
    public string CorrelationHeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets the OpenAPI document title.
    /// </summary>
    public string OpenApiTitle { get; set; } = "BusinessChronicle API";

    /// <summary>
    /// Gets or sets the OpenAPI document version.
    /// </summary>
    public string OpenApiVersion { get; set; } = "v1";
}
