namespace BusinessChronicle.React;

/// <summary>
/// React bridge integration helpers.
/// </summary>
public static class ReactBridgeExtensions
{
    /// <summary>
    /// Gets the recommended npm package name for React integration.
    /// </summary>
    public const string PackageName = "@business-chronicle/react";

    /// <summary>
    /// Gets the default OpenAPI document URL relative to the API host.
    /// </summary>
    public static string GetOpenApiUrl(string apiBaseUrl) =>
        $"{apiBaseUrl.TrimEnd('/')}{OpenApi.ChronicleOpenApiDocument.DocumentPath}";
}
