using Avalonia;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[CollectionDefinition]
public sealed class AvaloniaUiTests : ICollectionFixture<AvaloniaUiThread>;

public sealed class AvaloniaUiThread : IDisposable
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
    private readonly TaskCompletionSource<Dispatcher> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource shutdown = new();
    private readonly Thread thread;

    public AvaloniaUiThread()
    {
        var shutdownToken = shutdown.Token;
        thread = new Thread(() =>
        {
            try
            {
                AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
                Application.Current!.Styles.Add(new FluentTheme());
                var dispatcher = Dispatcher.UIThread;
                ready.SetResult(dispatcher);
                dispatcher.MainLoop(shutdownToken);
            }
            catch (Exception exception)
            {
                if (!ready.TrySetException(exception))
                {
                    throw;
                }
            }
        })
        {
            IsBackground = true
        };
        if (OperatingSystem.IsWindows())
        {
            thread.SetApartmentState(ApartmentState.STA);
        }

        thread.Start();
    }

    public async Task RunAsync(Func<Task> test)
    {
        var dispatcher = await ready.Task.WaitAsync(Timeout);
        await dispatcher.InvokeAsync(test).WaitAsync(Timeout * 4);
    }

    public void Dispose()
    {
        shutdown.Cancel();
        if (!thread.Join(Timeout))
        {
            throw new TimeoutException("The Avalonia dispatcher did not shut down.");
        }

        shutdown.Dispose();
    }
}
