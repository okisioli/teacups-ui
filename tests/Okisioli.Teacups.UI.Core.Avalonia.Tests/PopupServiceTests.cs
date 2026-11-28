using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class PopupServiceTests(AvaloniaUiThread uiThread)
{
    [Fact]
    public Task ShowToast_AlreadyShowing_DisplaysNextToastAfterFirstCloses()
    {
        return uiThread.RunAsync(async () =>
        {
            var root = new RootView();
            var window = new Window { Content = root, ShowInTaskbar = false };
            try
            {
                window.Show();
                var service = new PopupService(root);
                service.ShowToast("First");
                var first = Assert.IsAssignableFrom<Control>(root.ToastContent);
                service.ShowToast("Second", ToastDuration.Long);

                Assert.Same(first, root.ToastContent);
                Assert.Null(root.SnackbarContent);
                await WaitUntilAsync(() => root.ToastContent is not null && root.ToastContent != first);

                window.UpdateLayout();
                Assert.Contains(root.ToastContent!.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Second");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public Task ShowSnackbar_ReplacedAndActionSelected_InvokesOnlyLatestActionOnce()
    {
        return uiThread.RunAsync(async () =>
        {
            var root = new RootView();
            var window = new Window { Content = root, ShowInTaskbar = false };
            try
            {
                window.Show();
                var service = new PopupService(root);
                var replacedCount = 0;
                var actionCount = 0;
                service.ShowSnackbar("First", "Undo", () => replacedCount++);
                var first = Assert.IsAssignableFrom<Control>(root.SnackbarContent);
                var previousButton = Assert.Single(first.GetVisualDescendants().OfType<Button>());

                service.ShowSnackbar("Second", "Retry", () => actionCount++);
                previousButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Same(first, root.SnackbarContent);
                await WaitUntilAsync(() => root.SnackbarContent is not null && root.SnackbarContent != first);

                var actionButton = Assert.Single(root.SnackbarContent!.GetVisualDescendants().OfType<Button>());
                Assert.Equal("RETRY", actionButton.Content);
                actionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                actionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                await WaitUntilAsync(() => root.SnackbarContent is null);

                Assert.Equal(0, replacedCount);
                Assert.Equal(1, actionCount);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public Task ShowSnackbar_ShortDuration_TimesOutWithoutInvokingAction()
    {
        return uiThread.RunAsync(async () =>
        {
            var root = new RootView();
            var window = new Window { Content = root, ShowInTaskbar = false };
            try
            {
                window.Show();
                var service = new PopupService(root);
                var actionCount = 0;
                service.ShowSnackbar("Timed message", "Retry", () => actionCount++, SnackbarDuration.Short);
                Assert.NotNull(root.SnackbarContent);

                await WaitUntilAsync(() => root.SnackbarContent is null).WaitAsync(TimeSpan.FromSeconds(5));

                Assert.Equal(0, actionCount);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(AvaloniaUiThread.Timeout);
        while (!condition())
        {
            await Task.Delay(20, timeout.Token);
        }
    }
}
