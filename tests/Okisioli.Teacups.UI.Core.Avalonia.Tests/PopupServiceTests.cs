using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class PopupServiceTests(AvaloniaUiThread uiThread)
{
    [Fact]
    public Task ShowSnackbar_DismissedOrUndoSelected_InvokesCallbackOnlyOnceOnUndo()
    {
        return uiThread.RunAsync(() =>
        {
            var manager = new WindowNotificationManager();
            var window = new Window { Content = manager, ShowInTaskbar = false };
            try
            {
                window.Show();
                var service = new PopupService(manager);
                var undoCount = 0;

                service.ShowSnackbar("Dismissed message", () => undoCount++);
                window.UpdateLayout();
                var dismissedCard = Assert.Single(manager.GetVisualDescendants().OfType<NotificationCard>());
                dismissedCard.Close();
                Assert.Equal(0, undoCount);

                service.ShowSnackbar("Reversible message", () => undoCount++);
                window.UpdateLayout();
                var activeCard = Assert.Single(
                    manager.GetVisualDescendants().OfType<NotificationCard>(), card => !card.IsClosing);
                var undoButton = Assert.Single(activeCard.GetVisualDescendants().OfType<Button>());

                undoButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                undoButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.Equal(1, undoCount);
                Assert.True(activeCard.IsClosing);
            }
            finally
            {
                window.Close();
            }

            return Task.CompletedTask;
        });
    }
}
