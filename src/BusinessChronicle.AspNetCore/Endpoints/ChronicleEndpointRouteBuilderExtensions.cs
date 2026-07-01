using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.AspNetCore.ProblemDetails;
using BusinessChronicle.AspNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BusinessChronicle.AspNetCore.Endpoints;

/// <summary>
/// Minimal API endpoint mappings for BusinessChronicle.
/// </summary>
public static class ChronicleEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all BusinessChronicle endpoints.
    /// </summary>
    public static RouteGroupBuilder MapBusinessChronicle(this IEndpointRouteBuilder endpoints, string? prefix = null)
    {
        RouteGroupBuilder group = endpoints.MapGroup(prefix ?? "/api/chronicle")
            .WithTags("BusinessChronicle");

        group.MapChronicleHistory();
        group.MapChronicleCompare();
        group.MapChronicleRollback();
        group.MapTimeline();
        group.MapChronicleRevision();
        group.MapChronicleVersion();
        group.MapChronicleMetadata();
        group.MapChronicleComments();
        group.MapChronicleTags();

        return group;
    }

    /// <summary>
    /// Maps entity history endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleHistory(this RouteGroupBuilder group)
    {
        group.MapGet("/entities/{entityType}/{entityId}/history", async (
            string entityType,
            string entityId,
            IChronicleApiService service,
            int? maxResults,
            CancellationToken cancellationToken) =>
        {
            EntityReference entity = new() { Id = new EntityId(entityId), EntityType = entityType };
            TimelineQueryOptions options = new() { MaxResults = maxResults ?? 100 };
            var result = await service.GetHistoryAsync(entity, options, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("GetChronicleHistory")
        .WithSummary("Gets chronicle history entries for an entity.")
        .Produces<IReadOnlyList<ChronicleEntry>>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>
    /// Maps revision comparison endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleCompare(this RouteGroupBuilder group)
    {
        group.MapPost("/compare", async (
            ComparisonTarget target,
            IChronicleApiService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CompareAsync(target, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("CompareChronicleRevisions")
        .WithSummary("Compares two revisions or versions for an entity.")
        .Produces<Abstractions.Results.RevisionComparisonResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Maps rollback endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleRollback(this RouteGroupBuilder group)
    {
        group.MapPost("/entities/{entityType}/{entityId}/rollback/{revisionId}", async (
            string entityType,
            string entityId,
            string revisionId,
            CommitMessage message,
            IChronicleApiService service,
            CancellationToken cancellationToken) =>
        {
            EntityReference entity = new() { Id = new EntityId(entityId), EntityType = entityType };
            var result = await service.RollbackAsync(entity, new RevisionId(revisionId), message, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("RollbackChronicleEntity")
        .WithSummary("Rolls back an entity to a prior revision.")
        .Produces<Revision>()
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    /// <summary>
    /// Maps timeline endpoints.
    /// </summary>
    public static RouteGroupBuilder MapTimeline(this RouteGroupBuilder group)
    {
        group.MapGet("/entities/{entityType}/{entityId}/timeline", async (
            string entityType,
            string entityId,
            IChronicleApiService service,
            bool? descending,
            int? maxResults,
            CancellationToken cancellationToken) =>
        {
            EntityReference entity = new() { Id = new EntityId(entityId), EntityType = entityType };
            TimelineQueryOptions options = new() { Descending = descending ?? false, MaxResults = maxResults ?? 100 };
            var result = await service.GetTimelineAsync(entity, options, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("GetChronicleTimeline")
        .WithSummary("Gets ordered timeline entries for an entity.")
        .Produces<IReadOnlyList<TimelineEntry>>();

        return group;
    }

    /// <summary>
    /// Maps revision endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleRevision(this RouteGroupBuilder group)
    {
        group.MapGet("/revisions/{revisionId}", async (
            string revisionId,
            IChronicleApiService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetRevisionAsync(new RevisionId(revisionId), cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("GetChronicleRevision")
        .WithSummary("Gets a revision by identifier.")
        .Produces<Revision>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/entities/{entityType}/{entityId}/revisions", async (
            string entityType,
            string entityId,
            IChronicleApiService service,
            int? maxResults,
            CancellationToken cancellationToken) =>
        {
            EntityReference entity = new() { Id = new EntityId(entityId), EntityType = entityType };
            RevisionListOptions options = new() { MaxResults = maxResults ?? 100 };
            var result = await service.ListRevisionsAsync(entity, options, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("ListChronicleRevisions")
        .WithSummary("Lists revision references for an entity.")
        .Produces<IReadOnlyList<RevisionReference>>();

        return group;
    }

    /// <summary>
    /// Maps version head endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleVersion(this RouteGroupBuilder group)
    {
        group.MapGet("/entities/{entityType}/{entityId}/version", async (
            string entityType,
            string entityId,
            IChronicleApiService service,
            CancellationToken cancellationToken) =>
        {
            EntityReference entity = new() { Id = new EntityId(entityId), EntityType = entityType };
            var result = await service.GetHeadAsync(entity, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("GetChronicleHeadVersion")
        .WithSummary("Gets the current head revision for an entity.")
        .Produces<Revision>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    /// <summary>
    /// Maps metadata endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleMetadata(this RouteGroupBuilder group)
    {
        group.MapGet("/commits/{commitId}/metadata", async (
            string commitId,
            IChronicleApiService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetCommitAsync(new CommitId(commitId), cancellationToken).ConfigureAwait(false);
            return result.IsSuccess ? Results.Ok(result.Value!.Metadata) : ChronicleProblemDetailsMapper.FromFailure(result);
        })
        .WithName("GetChronicleCommitMetadata")
        .WithSummary("Gets metadata attached to a commit.")
        .Produces<ChronicleMetadata>();

        return group;
    }

    /// <summary>
    /// Maps comment endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleComments(this RouteGroupBuilder group)
    {
        group.MapGet("/revisions/{revisionId}/comments", (
            string revisionId) =>
            Results.Ok(Array.Empty<ChronicleComment>()))
        .WithName("GetChronicleComments")
        .WithSummary("Gets comments for a revision.")
        .Produces<IReadOnlyList<ChronicleComment>>();

        return group;
    }

    /// <summary>
    /// Maps tag endpoints.
    /// </summary>
    public static RouteGroupBuilder MapChronicleTags(this RouteGroupBuilder group)
    {
        group.MapGet("/revisions/{revisionId}/tags", (
            string revisionId) =>
            Results.Ok(Array.Empty<ChronicleTag>()))
        .WithName("GetChronicleTags")
        .WithSummary("Gets tags for a revision.")
        .Produces<IReadOnlyList<ChronicleTag>>();

        return group;
    }
}
