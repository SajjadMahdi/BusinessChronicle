using System.Runtime.CompilerServices;
using BusinessChronicle.EntityFrameworkCore.ChangeCapture;
using BusinessChronicle.EntityFrameworkCore.Configuration;
using BusinessChronicle.EntityFrameworkCore.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BusinessChronicle.EntityFrameworkCore.Interceptors;

/// <summary>
/// Captures entity changes during SaveChanges and persists chronicle records in the same transaction.
/// </summary>
public sealed class BusinessChronicleSaveChangesInterceptor(
    ChangeTrackerScanner scanner,
    BusinessChronicleEfOptions options) :
    ISaveChangesInterceptor
{
    /// <inheritdoc />
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            PrepareBatchAsync(eventData.Context, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        }

        return result;
    }

    /// <inheritdoc />
    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await PrepareBatchAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc />
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
        {
            DbContextChronicleState.Clear(eventData.Context);
        }
    }

    /// <inheritdoc />
    public Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            DbContextChronicleState.Clear(eventData.Context);
        }

        return Task.CompletedTask;
    }

    private async ValueTask PrepareBatchAsync(DbContext context, CancellationToken cancellationToken)
    {
        CommitCaptureBatch batch;
        try
        {
            batch = await scanner.ScanAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (options.TransactionBehavior == ChronicleTransactionBehavior.FailTransaction)
        {
            throw new InvalidOperationException("BusinessChronicle change capture failed.", ex);
        }

        if (batch.Changes.Count == 0)
        {
            return;
        }

        if (batch.Commit is not null)
        {
            context.Set<BcCommitEntity>().Add(batch.Commit);
        }

        context.Set<BcRevisionEntity>().AddRange(batch.Revisions);
        context.Set<BcDeltaEntity>().AddRange(batch.Deltas);
        context.Set<BcSnapshotEntity>().AddRange(batch.Snapshots);
        DbContextChronicleState.SetBatch(context, batch);
    }
}

internal sealed class BusinessChronicleDbContextState
{
    internal CommitCaptureBatch? Batch { get; set; }
}

internal static class DbContextChronicleState
{
    private static readonly ConditionalWeakTable<DbContext, BusinessChronicleDbContextState> States = new();

    internal static void SetBatch(DbContext context, CommitCaptureBatch batch)
    {
        States.GetOrCreateValue(context).Batch = batch;
    }

    internal static CommitCaptureBatch? GetBatch(DbContext context) =>
        States.TryGetValue(context, out BusinessChronicleDbContextState? state) ? state.Batch : null;

    internal static void Clear(DbContext context)
    {
        if (States.TryGetValue(context, out BusinessChronicleDbContextState? state))
        {
            state.Batch = null;
        }
    }
}
