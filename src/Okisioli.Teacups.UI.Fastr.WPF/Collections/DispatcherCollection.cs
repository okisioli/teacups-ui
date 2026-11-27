using System.Collections.ObjectModel;
using Okisioli.Teacups.UI.Core;
using DispatcherPriority = Okisioli.Teacups.UI.Core.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Fastr.WPF.Collections;

// TODO move to a standalone library
// TODO allow adjusting of priority
// TODO allow setting "batch size" for add & clear operations
/// <summary>An observable collection whose range operations queue changes on a UI dispatcher.</summary>
/// <typeparam name="T">The type of item stored in the collection.</typeparam>
/// <remarks>Call <see cref="BeginUpdate"/> and pass the returned version to a range operation.
/// Starting a newer update stops pending changes from older updates. Changes already applied stay in the collection.</remarks>
public class DispatcherCollection<T> : ObservableCollection<T>, INotifyCollectionReplacement
{
    private readonly IDispatcherService dispatcherService;
    private long currentUpdateVersion;
    private int busyOperationCount;

    /// <summary>Creates a collection that queues range changes through the specified dispatcher service.</summary>
    public DispatcherCollection(IDispatcherService dispatcherService)
    {
        this.dispatcherService = dispatcherService;
    }

    /// <inheritdoc />
    public event EventHandler? ReplacementStarting;

    /// <summary>Occurs when the first range operation begins or the last one ends.</summary>
    public event EventHandler? IsBusyChanged;

    /// <summary>Gets whether any range operations are still in progress.</summary>
    public bool IsBusy => Volatile.Read(ref busyOperationCount) > 0;

    /// <summary>Starts a new update and returns its version, superseding earlier versions.</summary>
    public long BeginUpdate()
    {
        return Interlocked.Increment(ref currentUpdateVersion);
    }

    /// <summary>Returns whether the specified update version is still current.</summary>
    public bool IsCurrent(long updateVersion)
    {
        return Volatile.Read(ref currentUpdateVersion) == updateVersion;
    }

    /// <summary>Appends items incrementally while the specified update version is current.</summary>
    public async Task AddRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var busyScope = BeginBusyScope();
        using var enumerator = newItems.GetEnumerator();

        while (IsCurrent(updateVersion) && enumerator.MoveNext())
        {
            var item = enumerator.Current;

            await dispatcherService.InvokeAsync(() =>
            {
                if (IsCurrent(updateVersion))
                    Add(item);
            }, DispatcherPriority.Background);
        }
    }

    /// <summary>Signals a replacement and incrementally replaces the existing items while the update version is current.</summary>
    public async Task ReplaceRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var busyScope = BeginBusyScope();

        ReplacementStarting?.Invoke(this, EventArgs.Empty);
        var materializedItems = newItems.ToList();
        var oldItemCount = await GetCountAsync(updateVersion);

        if (!IsCurrent(updateVersion))
            return;

        if (oldItemCount == 0)
        {
            await AddRangeAsync(materializedItems, updateVersion);
            return;
        }

        await RemoveFirstItemAsync(updateVersion);
        oldItemCount--;

        var clearTask = RemoveFirstItemsAsync(updateVersion, oldItemCount);
        await AddRangeAsync(materializedItems, updateVersion);
        await clearTask;
    }

    /// <summary>Removes items incrementally while the specified update version is current.</summary>
    public async Task ClearAsync(long updateVersion)
    {
        using var busyScope = BeginBusyScope();

        while (IsCurrent(updateVersion))
        {
            var removedItem = false;

            await dispatcherService.InvokeAsync(() =>
            {
                if (!IsCurrent(updateVersion) || Count == 0)
                    return;

                RemoveAt(0);
                removedItem = true;
            }, DispatcherPriority.Background);

            if (!removedItem)
                break;
        }
    }

    private async Task<int> GetCountAsync(long updateVersion)
    {
        var count = 0;

        await dispatcherService.InvokeAsync(() =>
        {
            if (IsCurrent(updateVersion))
                count = Count;
        }, DispatcherPriority.Background);

        return count;
    }

    private async Task RemoveFirstItemsAsync(long updateVersion, int count)
    {
        for (var i = 0; IsCurrent(updateVersion) && i < count; i++)
            await RemoveFirstItemAsync(updateVersion);
    }

    private async Task RemoveFirstItemAsync(long updateVersion)
    {
        await dispatcherService.InvokeAsync(() =>
        {
            if (IsCurrent(updateVersion) && Count > 0)
                RemoveAt(0);
        }, DispatcherPriority.Background);
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
        private readonly DispatcherCollection<T> collection;
        private bool disposed;

        public BusyScope(DispatcherCollection<T> collection)
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
