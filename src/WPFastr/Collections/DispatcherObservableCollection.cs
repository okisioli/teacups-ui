using System.Collections.ObjectModel;
using Olicode.WPFastr.Threading;

namespace Olicode.WPFastr.Collections;

public class DispatcherObservableCollection<T> : ObservableCollection<T>
{
    private readonly IDispatcherService dispatcherService;
    private long currentUpdateVersion;
    private int busyOperationCount;

    public DispatcherObservableCollection(
        IDispatcherService dispatcherService,
        int batchSize = 1,
        DispatchPriority dispatchPriority = DispatchPriority.Background)
    {
        if (batchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be at least one.");

        this.dispatcherService = dispatcherService;
        BatchSize = batchSize;
        DispatchPriority = dispatchPriority;
    }

    public event EventHandler? IsBusyChanged;

    public int BatchSize { get; }

    public DispatchPriority DispatchPriority { get; }

    public bool IsBusy => Volatile.Read(ref busyOperationCount) > 0;

    public long BeginUpdate()
    {
        return Interlocked.Increment(ref currentUpdateVersion);
    }

    public bool IsCurrent(long updateVersion)
    {
        return Volatile.Read(ref currentUpdateVersion) == updateVersion;
    }

    public async Task AddRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var busyScope = BeginBusyScope();
        using var enumerator = newItems.GetEnumerator();

        while (IsCurrent(updateVersion))
        {
            var batch = TakeBatch(enumerator);

            if (batch.Count == 0)
                return;

            await dispatcherService.InvokeAsync(() =>
            {
                foreach (var item in batch)
                {
                    if (!IsCurrent(updateVersion))
                        return;

                    Add(item);
                }
            }, DispatchPriority);
        }
    }

    public async Task ReplaceRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var busyScope = BeginBusyScope();

        var materializedItems = newItems.ToList();
        var oldItemCount = await GetCountAsync(updateVersion);

        if (!IsCurrent(updateVersion))
            return;

        if (oldItemCount == 0)
        {
            await AddRangeAsync(materializedItems, updateVersion);
            return;
        }

        await RemoveFirstItemsAsync(updateVersion, 1);
        oldItemCount--;

        var clearTask = RemoveFirstItemsAsync(updateVersion, oldItemCount);
        await AddRangeAsync(materializedItems, updateVersion);
        await clearTask;
    }

    public async Task ClearAsync(long updateVersion)
    {
        using var busyScope = BeginBusyScope();
        await RemoveFirstItemsAsync(updateVersion, int.MaxValue);
    }

    private List<T> TakeBatch(IEnumerator<T> enumerator)
    {
        var batch = new List<T>(BatchSize);

        while (batch.Count < BatchSize && enumerator.MoveNext())
            batch.Add(enumerator.Current);

        return batch;
    }

    private async Task<int> GetCountAsync(long updateVersion)
    {
        var count = 0;

        await dispatcherService.InvokeAsync(() =>
        {
            if (IsCurrent(updateVersion))
                count = Count;
        }, DispatchPriority);

        return count;
    }

    private async Task RemoveFirstItemsAsync(long updateVersion, int count)
    {
        var remaining = count;

        while (IsCurrent(updateVersion) && remaining > 0)
        {
            var removedCount = 0;

            await dispatcherService.InvokeAsync(() =>
            {
                while (IsCurrent(updateVersion) &&
                       removedCount < BatchSize &&
                       removedCount < remaining &&
                       Count > 0)
                {
                    RemoveAt(0);
                    removedCount++;
                }
            }, DispatchPriority);

            if (removedCount == 0)
                return;

            remaining -= removedCount;
        }
    }

    private IDisposable BeginBusyScope()
    {
        if (Interlocked.Increment(ref busyOperationCount) == 1)
            IsBusyChanged?.Invoke(this, EventArgs.Empty);

        return new BusyScope(this);
    }

    private void EndBusyScope()
    {
        if (Interlocked.Decrement(ref busyOperationCount) == 0)
            IsBusyChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class BusyScope : IDisposable
    {
        private readonly DispatcherObservableCollection<T> collection;
        private bool disposed;

        public BusyScope(DispatcherObservableCollection<T> collection)
        {
            this.collection = collection;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            collection.EndBusyScope();
        }
    }
}
