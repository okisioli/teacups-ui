using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>
/// Shows three-second toasts and six-second snackbars using Avalonia notifications.
/// </summary>
/// <param name="notificationManager">The themed notification manager hosted in the application's visual tree.</param>
/// <remarks>Call this service from the UI thread after the notification manager is attached.</remarks>
public sealed class PopupService(WindowNotificationManager notificationManager) : IPopupService
{
    /// <inheritdoc />
    public void ShowToast(string text)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(text);

        var message = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12)
        };
        notificationManager.ApplyTemplate();
        notificationManager.Show(message, NotificationType.Information, TimeSpan.FromSeconds(3));
    }

    /// <inheritdoc />
    public void ShowSnackbar(string text, Action undoAction)
    {
        Dispatcher.UIThread.VerifyAccess();
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(undoAction);

        var undoButton = new Button
        {
            Content = "Undo",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };
        undoButton.Click += (_, _) =>
        {
            if (undoButton.FindAncestorOfType<NotificationCard>() is { IsClosing: false } card)
            {
                card.Close();
                undoAction();
            }
        };
        DockPanel.SetDock(undoButton, Dock.Right);

        var content = new DockPanel
        {
            Margin = new Thickness(12),
            Children =
            {
                undoButton,
                new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
        notificationManager.ApplyTemplate();
        notificationManager.Show(content, NotificationType.Information, TimeSpan.FromSeconds(6));
    }
}
