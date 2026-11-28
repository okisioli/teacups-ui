using Avalonia.Animation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Okisioli.Teacups.UI.Controls.Avalonia;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>
/// Shows queued toasts and one snackbar at a time in a window-level overlay, independently of
/// Avalonia's WindowNotificationManager, which is designed for stacked notifications.
/// </summary>
/// <remarks>Use one service per <see cref="RootView"/> and call it on the UI thread after the view is attached.</remarks>
public sealed class PopupService : IPopupService
{
    private static readonly TimeSpan ToastDuration = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan SnackbarDuration = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(150);

    private readonly RootView root;
    private readonly Queue<string> toasts = new();
    private Snackbar? pendingSnackbar;
    private TaskCompletionSource? dismissSnackbar;
    private CancellationTokenSource? activeRun;

    /// <summary>Initializes the service for the specified window or application view.</summary>
    /// <param name="root">The root view that owns the popup layers.</param>
    public PopupService(RootView root)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        root.DetachedFromVisualTree += (_, _) =>
        {
            toasts.Clear();
            pendingSnackbar = null;
            activeRun?.Cancel();
        };
    }

    /// <inheritdoc />
    public void ShowToast(string text)
    {
        VerifyHost();
        ArgumentNullException.ThrowIfNull(text);

        toasts.Enqueue(text);
        PresentQueuedPopups();
    }

    /// <inheritdoc />
    public void ShowSnackbar(string text, string actionText, Action action)
    {
        VerifyHost();
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionText);
        ArgumentNullException.ThrowIfNull(action);

        pendingSnackbar = new Snackbar(text, actionText, action);
        dismissSnackbar?.TrySetResult();
        PresentQueuedPopups();
    }

    private void VerifyHost()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (!root.IsAttachedToVisualTree())
        {
            throw new InvalidOperationException("The popup root must be attached to a window or application view.");
        }
    }

    private async void PresentQueuedPopups()
    {
        if (activeRun is not null)
        {
            return;
        }

        using var run = new CancellationTokenSource();
        activeRun = run;
        try
        {
            while (!run.IsCancellationRequested)
            {
                if (pendingSnackbar is { } snackbar)
                {
                    pendingSnackbar = null;
                    await PresentSnackbarAsync(snackbar, run.Token);
                }
                else if (toasts.TryDequeue(out var text))
                {
                    await PresentToastAsync(text, run.Token);
                }
                else
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (run.IsCancellationRequested)
        {
        }
        finally
        {
            dismissSnackbar = null;
            root.SnackbarContent = null;
            root.ToastContent = null;
            activeRun = null;

            if (pendingSnackbar is not null || toasts.Count > 0)
            {
                PresentQueuedPopups();
            }
        }
    }

    private async Task PresentToastAsync(string text, CancellationToken cancellationToken)
    {
        var popup = new PopupView(text);
        var fade = (IPageTransition)new CrossFade(AnimationDuration);
        root.ToastContent = popup;
        await fade.Start(null, popup, true, cancellationToken);
        await Task.Delay(ToastDuration, cancellationToken);
        await fade.Start(popup, null, false, cancellationToken);
        root.ToastContent = null;
    }

    private async Task PresentSnackbarAsync(Snackbar snackbar, CancellationToken cancellationToken)
    {
        var dismissed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dismissSnackbar = dismissed;
        var popup = new PopupView(snackbar.Text, snackbar.ActionText, () =>
        {
            if (dismissed.TrySetResult())
            {
                snackbar.Action();
            }
        });
        var slideAndFade = new CompositePageTransition
        {
            PageTransitions =
            {
                new PageSlide(AnimationDuration, PageSlide.SlideAxis.Vertical),
                new CrossFade(AnimationDuration)
            }
        };

        root.SnackbarContent = popup;
        // PageSlide measures the popup's arranged parent to determine the travel distance.
        root.UpdateLayout();
        await slideAndFade.Start(null, popup, true, cancellationToken);
        if (!dismissed.Task.IsCompleted)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await Task.WhenAny(dismissed.Task, Task.Delay(SnackbarDuration, timeout.Token));
            timeout.Cancel();
        }

        dismissed.TrySetResult();
        cancellationToken.ThrowIfCancellationRequested();
        await slideAndFade.Start(popup, null, false, cancellationToken);
        root.SnackbarContent = null;
        dismissSnackbar = null;
    }

    private sealed record Snackbar(string Text, string ActionText, Action Action);
}
