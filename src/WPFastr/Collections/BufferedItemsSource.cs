using System.Collections;
using System.ComponentModel;
using Olicode.WPFastr.Threading;

namespace Olicode.WPFastr.Collections;

public sealed class BufferedItemsSource<T> : IBufferedItemsSource
{
    private readonly IDispatcherService dispatcherService;
    private long currentUpdateVersion;
    private int busyOperationCount;

    public BufferedItemsSource(
        IDispatcherService dispatcherService,
        DispatchPriority dispatchPriority = DispatchPriority.Background)
    {
        this.dispatcherService = dispatcherService;
        DispatchPriority = dispatchPriority;
        VisibleItems = Array.Empty<T>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? IsBusyChanged;

    public IReadOnlyList<T> VisibleItems { get; private set; }

    IEnumerable IBufferedItemsSource.VisibleItems => VisibleItems;

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

    public async Task ReplaceRangeAsync(IEnumerable<T> newItems, long updateVersion)
    {
        using var busyScope = BeginBusyScope();
        var nextItems = newItems.ToArray();

        if (!IsCurrent(updateVersion))
            return;

        await dispatcherService.InvokeAsync(() =>
        {
            if (!IsCurrent(updateVersion))
                return;

            VisibleItems = nextItems;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleItems)));
        }, DispatchPriority);
    }

    public Task ClearAsync(long updateVersion)
    {
        return ReplaceRangeAsync(Array.Empty<T>(), updateVersion);
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
        private readonly BufferedItemsSource<T> source;
        private bool disposed;

        public BusyScope(BufferedItemsSource<T> source)
        {
            this.source = source;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            source.EndBusyScope();
        }
    }
}
