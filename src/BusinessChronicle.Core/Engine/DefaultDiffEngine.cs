using System.Text.Json;
using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Abstractions.Results;
using BusinessChronicle.Storage.InMemory;

namespace BusinessChronicle.Engine;

/// <summary>
/// Default diff engine that compares revision snapshots and metadata.
/// </summary>
public sealed class DefaultDiffEngine(IChronicleStore store) : IChronicleDiffEngine
{
    /// <inheritdoc />
    public async ValueTask<ChronicleResult<IReadOnlyList<ChangeDescriptor>>> DiffAsync(
        ComparisonTarget target,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ChronicleResult<Revision> sourceResult = await ResolveRevisionAsync(target, source: true, cancellationToken)
            .ConfigureAwait(false);
        if (sourceResult.IsFailure)
        {
            return ChronicleResults.Failure<IReadOnlyList<ChangeDescriptor>>(sourceResult.Error!.Value);
        }

        ChronicleResult<Revision> targetResult = await ResolveRevisionAsync(target, source: false, cancellationToken)
            .ConfigureAwait(false);
        if (targetResult.IsFailure)
        {
            return ChronicleResults.Failure<IReadOnlyList<ChangeDescriptor>>(targetResult.Error!.Value);
        }

        List<ChangeDescriptor> changes = [];
        changes.AddRange(await CompareSnapshotsAsync(sourceResult.Value!, targetResult.Value!, cancellationToken)
            .ConfigureAwait(false));

        return ChronicleResults.Success<IReadOnlyList<ChangeDescriptor>>(changes);
    }

    private async ValueTask<ChronicleResult<Revision>> ResolveRevisionAsync(
        ComparisonTarget target,
        bool source,
        CancellationToken cancellationToken)
    {
        RevisionId? revisionId = source ? target.SourceRevisionId : target.TargetRevisionId;
        if (revisionId is { } id)
        {
            return await store.RevisionReader.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        }

        VersionPointer? version = source ? target.SourceVersion : target.TargetVersion;
        if (version is null)
        {
            return ChronicleResults.Failure<Revision>(
                new ChronicleError(ChronicleErrorCode.ValidationFailed, "Comparison endpoint was not specified."));
        }

        if (store is InMemoryChronicleStore inMemory)
        {
            Revision? revision = inMemory.State.FindRevisionByVersion(target.Entity, version.Value);
            return revision is null
                ? ChronicleResults.Failure<Revision>(
                    new ChronicleError(ChronicleErrorCode.NotFound, "Revision for the requested version was not found."))
                : ChronicleResults.Success(revision);
        }

        ChronicleResult<IReadOnlyList<RevisionReference>> listResult = await store.RevisionReader
            .ListAsync(target.Entity, new RevisionListOptions { MaxResults = int.MaxValue }, cancellationToken)
            .ConfigureAwait(false);

        if (listResult.IsFailure)
        {
            return ChronicleResults.Failure<Revision>(listResult.Error!.Value);
        }

        foreach (RevisionReference reference in listResult.Value!)
        {
            ChronicleResult<Revision> revisionResult = await store.RevisionReader
                .ReadAsync(reference.Id, cancellationToken)
                .ConfigureAwait(false);

            if (revisionResult.IsSuccess &&
                revisionResult.Value!.Version?.Number == version.Value.Number)
            {
                return revisionResult;
            }
        }

        return ChronicleResults.Failure<Revision>(
            new ChronicleError(ChronicleErrorCode.NotFound, "Revision for the requested version was not found."));
    }

    private async ValueTask<IReadOnlyList<ChangeDescriptor>> CompareSnapshotsAsync(
        Revision source,
        Revision target,
        CancellationToken cancellationToken)
    {
        ChronicleResult<Snapshot> sourceSnapshotResult = await store.RevisionReader
            .ReadSnapshotAsync(source.Id, cancellationToken)
            .ConfigureAwait(false);

        ChronicleResult<Snapshot> targetSnapshotResult = await store.RevisionReader
            .ReadSnapshotAsync(target.Id, cancellationToken)
            .ConfigureAwait(false);

        if (sourceSnapshotResult.IsFailure || targetSnapshotResult.IsFailure)
        {
            return [];
        }

        ReadOnlyMemory<byte> sourcePayload = sourceSnapshotResult.Value!.Payload;
        ReadOnlyMemory<byte> targetPayload = targetSnapshotResult.Value!.Payload;

        if (sourcePayload.Span.SequenceEqual(targetPayload.Span))
        {
            return [];
        }

        try
        {
            using JsonDocument sourceDocument = JsonDocument.Parse(sourcePayload);
            using JsonDocument targetDocument = JsonDocument.Parse(targetPayload);
            return DiffJsonElements(sourceDocument.RootElement, targetDocument.RootElement, "$");
        }
        catch (JsonException)
        {
            return
            [
                new ChangeDescriptor
                {
                    Path = "$",
                    Kind = ChangeKind.Modified,
                },
            ];
        }
    }

    private static List<ChangeDescriptor> DiffJsonElements(JsonElement source, JsonElement target, string path)
    {
        List<ChangeDescriptor> changes = [];

        if (source.ValueKind != target.ValueKind)
        {
            changes.Add(new ChangeDescriptor { Path = path, Kind = ChangeKind.Modified });
            return changes;
        }

        switch (source.ValueKind)
        {
            case JsonValueKind.Object:
                HashSet<string> propertyNames = new(StringComparer.Ordinal);
                foreach (JsonProperty property in source.EnumerateObject())
                {
                    propertyNames.Add(property.Name);
                }

                foreach (JsonProperty property in target.EnumerateObject())
                {
                    propertyNames.Add(property.Name);
                }

                foreach (string name in propertyNames)
                {
                    string childPath = $"{path}.{name}";
                    if (!source.TryGetProperty(name, out JsonElement sourceChild))
                    {
                        changes.Add(new ChangeDescriptor { Path = childPath, Kind = ChangeKind.Added });
                        continue;
                    }

                    if (!target.TryGetProperty(name, out JsonElement targetChild))
                    {
                        changes.Add(new ChangeDescriptor { Path = childPath, Kind = ChangeKind.Removed });
                        continue;
                    }

                    changes.AddRange(DiffJsonElements(sourceChild, targetChild, childPath));
                }

                break;

            case JsonValueKind.Array:
                if (source.GetArrayLength() != target.GetArrayLength())
                {
                    changes.Add(new ChangeDescriptor { Path = path, Kind = ChangeKind.Modified });
                }

                break;

            default:
                if (source.GetRawText() != target.GetRawText())
                {
                    changes.Add(new ChangeDescriptor
                    {
                        Path = path,
                        Kind = ChangeKind.Modified,
                        PropertyChange = new PropertyChange
                        {
                            PropertyName = path,
                            OldValue = source.GetRawText(),
                            NewValue = target.GetRawText(),
                        },
                    });
                }

                break;
        }

        return changes;
    }
}
