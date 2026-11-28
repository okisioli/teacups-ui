using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;

namespace Okisioli.Teacups.UI.Core.WPF.Tests;

public sealed class DialogServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ShowDialogAsync_CompletedFromView_ClosesWindowAndReturnsResult(bool confirm)
    {
        return RunDialogTestAsync((_, completion) =>
        {
            if (confirm)
            {
                completion.Confirm();
            }
            else
            {
                completion.Cancel();
            }
        }, confirm);
    }

    [Fact]
    public Task ShowDialogAsync_WindowClosed_ReturnsFalse()
    {
        return RunDialogTestAsync((window, _) => window.Close(), expectedResult: false);
    }

    [Theory]
    [InlineData(false, WindowState.Normal)]
    [InlineData(true, WindowState.Maximized)]
    public Task ShowDialogAsync_FullScreenConfigured_UsesExpectedWindowState(bool fullScreen, WindowState expected)
    {
        return RunDialogTestAsync((window, _) =>
        {
            Assert.Equal(expected, window.WindowState);
            window.Close();
        }, expectedResult: false, fullScreen: fullScreen);
    }

    private static async Task RunDialogTestAsync(
        Action<Window, IDialogCompletion> dismiss, bool expectedResult, bool fullScreen = false)
    {
        var dispatcherReady = new TaskCompletionSource<Dispatcher>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            dispatcherReady.SetResult(Dispatcher.CurrentDispatcher);
            Dispatcher.Run();
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        var dispatcher = await dispatcherReady.Task.WaitAsync(Timeout);
        try
        {
            var test = dispatcher.InvokeAsync(() => CheckDialogAsync(dispatcher, dismiss, expectedResult, fullScreen));
            await test.Task.Unwrap().WaitAsync(Timeout + Timeout);
        }
        finally
        {
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }
    }

    private static async Task CheckDialogAsync(
        Dispatcher dispatcher, Action<Window, IDialogCompletion> dismiss, bool expectedResult, bool fullScreen)
    {
        var loaded = new TaskCompletionSource<Window>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new DialogService<TestViewModel, TestView>(dispatcher) { FullScreen = fullScreen };
        TestViewModel? model = null;
        var result = service.ShowDialogAsync(completion => model = new TestViewModel(completion, loaded));
        var first = await Task.WhenAny(loaded.Task, result).WaitAsync(Timeout);
        if (first == result)
        {
            await result;
            Assert.Fail("The dialog completed before its view loaded.");
        }

        var window = await loaded.Task;
        try
        {
            Assert.True(window.IsVisible);
            Assert.False(result.IsCompleted);
            var closed = false;
            window.Closed += (_, _) => closed = true;

            dismiss(window, Assert.IsType<TestViewModel>(model).Completion);

            Assert.Equal(expectedResult, await result.WaitAsync(Timeout));
            Assert.True(closed);
        }
        finally
        {
            if (window.IsVisible)
            {
                window.Close();
            }
        }
    }

    private sealed record TestViewModel(IDialogCompletion Completion, TaskCompletionSource<Window> Loaded);

    private sealed class TestView : UserControl
    {
        public TestView()
        {
            Loaded += (_, _) =>
            {
                if (DataContext is TestViewModel model && Window.GetWindow(this) is { } window)
                {
                    model.Loaded.TrySetResult(window);
                }
            };
        }
    }
}
