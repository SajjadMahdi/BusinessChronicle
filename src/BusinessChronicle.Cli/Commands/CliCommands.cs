using System.CommandLine;
using BusinessChronicle.Abstractions.Identifiers;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.Cli.Internal;
using BusinessChronicle.Engine;
using BusinessChronicle.Testing;
using BusinessChronicle.Testing.Assertions;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Cli.Commands;

internal static class DoctorCommand
{
    internal static Command Create()
    {
        Command command = new("doctor", "Checks BusinessChronicle configuration and dependencies.");
        command.SetHandler(() =>
        {
            using ServiceProvider provider = InMemoryCliStore.BuildProvider();
            _ = provider.GetRequiredService<Abstractions.Contracts.IChronicleStore>();
            Console.WriteLine("BusinessChronicle CLI: OK");
        });

        return command;
    }
}

internal static class ListCommand
{
    internal static Command Create()
    {
        Argument<string> entityType = new("entity-type", "Entity type name.");
        Argument<string> entityId = new("entity-id", "Entity identifier.");
        Command command = new("list", "Lists revisions for an entity.") { entityType, entityId };
        command.SetHandler(async (string type, string id) =>
        {
            using ServiceProvider provider = InMemoryCliStore.BuildProvider();
            var store = provider.GetRequiredService<Abstractions.Contracts.IChronicleStore>();
            EntityReference entity = new() { Id = new EntityId(id), EntityType = type };
            var result = await store.RevisionReader.ListAsync(entity, new RevisionListOptions(), CancellationToken.None);
            if (result.IsFailure)
            {
                Console.Error.WriteLine(result.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            foreach (RevisionReference revision in result.Value!)
            {
                Console.WriteLine(revision.Id.Value);
            }
        }, entityType, entityId);

        return command;
    }
}

internal static class HistoryCommand
{
    internal static Command Create()
    {
        Argument<string> entityType = new("entity-type", "Entity type name.");
        Argument<string> entityId = new("entity-id", "Entity identifier.");
        Command command = new("history", "Shows history entries for an entity.") { entityType, entityId };
        command.SetHandler(async (string type, string id) =>
        {
            using ServiceProvider provider = InMemoryCliStore.BuildProvider();
            var store = provider.GetRequiredService<Abstractions.Contracts.IChronicleStore>();
            EntityReference entity = new() { Id = new EntityId(id), EntityType = type };
            var result = await store.Query.GetEntriesAsync(entity, new TimelineQueryOptions(), CancellationToken.None);
            if (result.IsFailure)
            {
                Console.Error.WriteLine(result.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            foreach (ChronicleEntry entry in result.Value!)
            {
                Console.WriteLine($"{entry.OccurredAt:O} {entry.RevisionType} {entry.RevisionId.Value}");
            }
        }, entityType, entityId);

        return command;
    }
}

internal static class CompareCommand
{
    internal static Command Create()
    {
        Argument<string> entityType = new("entity-type", "Entity type name.");
        Argument<string> entityId = new("entity-id", "Entity identifier.");
        Argument<string> sourceRevision = new("source-revision", "Source revision id.");
        Argument<string> targetRevision = new("target-revision", "Target revision id.");
        Command command = new("compare", "Compares two revisions.") { entityType, entityId, sourceRevision, targetRevision };
        command.SetHandler(async (string type, string id, string source, string target) =>
        {
            using ServiceProvider provider = InMemoryCliStore.BuildProvider();
            var comparer = provider.GetRequiredService<Abstractions.Contracts.IRevisionComparer>();
            ComparisonTarget comparison = new()
            {
                Entity = new EntityReference { Id = new EntityId(id), EntityType = type },
                SourceRevisionId = new RevisionId(source),
                TargetRevisionId = new RevisionId(target),
            };

            var result = await comparer.CompareAsync(comparison, CancellationToken.None);
            if (result.IsFailure)
            {
                Console.Error.WriteLine(result.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"Changes: {result.Value!.Changes.Count}");
        }, entityType, entityId, sourceRevision, targetRevision);

        return command;
    }
}

internal static class RollbackCommand
{
    internal static Command Create()
    {
        Argument<string> entityType = new("entity-type", "Entity type name.");
        Argument<string> entityId = new("entity-id", "Entity identifier.");
        Argument<string> revisionId = new("revision-id", "Target revision id.");
        Command command = new("rollback", "Rolls back an entity to a revision.") { entityType, entityId, revisionId };
        command.SetHandler(async (string type, string id, string revision) =>
        {
            await using ChronicleTestContext context = ChronicleTestContext.Create();
            EntityReference entity = new() { Id = new EntityId(id), EntityType = type };
            var sessionResult = await context.CreateSessionAsync(entity);
            if (sessionResult.IsFailure)
            {
                Console.Error.WriteLine(sessionResult.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            await using var session = sessionResult.Value!;
            if (session is not ChronicleSession concrete)
            {
                Console.Error.WriteLine("Unsupported session implementation.");
                Environment.ExitCode = 1;
                return;
            }

            var rollback = await concrete.RollbackAsync(
                entity,
                new RevisionId(revision),
                new CommitMessage { Text = "CLI rollback" },
                CancellationToken.None);

            if (rollback.IsFailure)
            {
                Console.Error.WriteLine(rollback.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            ChronicleAssertions.AssertSuccess(await session.CommitAsync());
            Console.WriteLine($"Rolled back to {revision}");
        }, entityType, entityId, revisionId);

        return command;
    }
}

internal static class ExportCommand
{
    internal static Command Create()
    {
        Argument<string> entityType = new("entity-type", "Entity type name.");
        Argument<string> entityId = new("entity-id", "Entity identifier.");
        Option<FileInfo> output = new("--output", () => new FileInfo("chronicle-export.json"), "Output file path.");
        Command command = new("export", "Exports entity history to JSON.") { entityType, entityId, output };
        command.SetHandler(async (string type, string id, FileInfo file) =>
        {
            using ServiceProvider provider = InMemoryCliStore.BuildProvider();
            var store = provider.GetRequiredService<Abstractions.Contracts.IChronicleStore>();
            EntityReference entity = new() { Id = new EntityId(id), EntityType = type };
            var result = await store.Query.GetEntriesAsync(entity, new TimelineQueryOptions { MaxResults = 1000 }, CancellationToken.None);
            if (result.IsFailure)
            {
                Console.Error.WriteLine(result.Error?.Message);
                Environment.ExitCode = 1;
                return;
            }

            await File.WriteAllTextAsync(file.FullName, $"{{\"count\":{result.Value!.Count}}}");
            Console.WriteLine($"Exported to {file.FullName}");
        }, entityType, entityId, output);

        return command;
    }
}

internal static class InfoCommand
{
    internal static Command Create()
    {
        Command command = new("info", "Shows package and runtime information.");
        command.SetHandler(() =>
        {
            Console.WriteLine($"BusinessChronicle CLI {typeof(InfoCommand).Assembly.GetName().Version}");
            Console.WriteLine($"Runtime: {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion}");
        });

        return command;
    }
}
