using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace BusinessChronicle.AspNetCore.Http;

/// <summary>
/// Resolves the current actor from HTTP context claims and headers.
/// </summary>
public sealed class HttpContextActorResolver(IHttpContextAccessor httpContextAccessor) : Abstractions.Contracts.IActorResolver
{
    /// <inheritdoc />
    public ValueTask<Actor> ResolveAsync(CancellationToken cancellationToken = default)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated == true)
        {
            string id = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.User.Identity.Name
                ?? "authenticated-user";

            string? displayName = context.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? context.User.Identity.Name;

            return ValueTask.FromResult(new Actor
            {
                Id = id,
                Type = ActorType.User,
                DisplayName = displayName,
            });
        }

        return ValueTask.FromResult(new Actor
        {
            Id = "anonymous",
            Type = ActorType.Anonymous,
            DisplayName = "Anonymous",
        });
    }
}

/// <summary>
/// Provides correlation identifiers from the current HTTP request.
/// </summary>
public sealed class HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor, Microsoft.Extensions.Options.IOptions<Options.BusinessChronicleAspNetCoreOptions> options)
{
    /// <summary>
    /// Gets the correlation identifier for the current request.
    /// </summary>
    public string? GetCorrelationId()
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        string headerName = options.Value.CorrelationHeaderName;
        if (context.Request.Headers.TryGetValue(headerName, out Microsoft.Extensions.Primitives.StringValues value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value.ToString();
        }

        return context.TraceIdentifier;
    }
}
