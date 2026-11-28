using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>
/// Shows Avalonia user controls in a centered drawer sheet or as full-screen navigation pages.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">A <see cref="UserControl"/> with a public parameterless constructor.</typeparam>
/// <param name="navigationPage">The navigation page that hosts the dialog.</param>
/// <param name="dialogSheet">The shell's drawer sheet reserved for dialogs.</param>
/// <param name="confirmOnBackNavigation">
/// <see langword="true"/> to confirm when back navigation pops the dialog page.
/// <see langword="false"/> to cancel.
/// </param>
/// <remarks>
/// For the default centered presentation, wrap the navigation page in the dialog sheet at app startup.
/// Dismissing the sheet returns false. Escape also cancels full-screen dialogs.
/// Set <see cref="Button.IsDefault"/> on the view's confirmation button
/// to enable Enter.
/// </remarks>
public class DialogService<TViewModel, TView>(NavigationPage navigationPage, DrawerSheet dialogSheet, bool confirmOnBackNavigation) : IDialogService<TViewModel, TView>
    where TView : UserControl, new()
{
    /// <inheritdoc />
    public bool FullScreen { get; set; }

    /// <inheritdoc />
    public Task<bool> ShowDialogAsync(TViewModel viewModel)
    {
        return ShowDialogAsync(_ => viewModel);
    }

    /// <inheritdoc />
    public Task<bool> ShowDialogAsync(Func<IDialogCompletion, TViewModel> viewModelFactory)
    {
        var completion = new DialogCompletion();
        var viewModel = viewModelFactory(completion);
        var view = new TView
        {
            DataContext = viewModel
        };

        return FullScreen ? ShowPageAsync(view, completion) : ShowSheetAsync(view, completion);
    }

    private async Task<bool> ShowSheetAsync(TView view, DialogCompletion completion)
    {
        if (!dialogSheet.IsAttachedToVisualTree() || dialogSheet.SheetContent is not null || dialogSheet.IsExpanded)
        {
            return false;
        }

        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnSheetChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == DrawerSheet.IsExpandedProperty && !dialogSheet.IsExpanded)
            {
                completion.Cancel();
            }
        }

        void OnSheetClosed(object? sender, EventArgs args) => closed.TrySetResult();

        void OnSheetDetached(object? sender, VisualTreeAttachmentEventArgs args)
        {
            completion.Cancel();
            closed.TrySetResult();
        }

        dialogSheet.PropertyChanged += OnSheetChanged;
        dialogSheet.SheetClosed += OnSheetClosed;
        dialogSheet.DetachedFromVisualTree += OnSheetDetached;
        try
        {
            dialogSheet.SheetHorizontalAlignment = HorizontalAlignment.Center;
            dialogSheet.SheetVerticalAlignment = VerticalAlignment.Center;
            dialogSheet.SheetContent = view;
            dialogSheet.IsExpanded = true;
            var result = await completion.Result;
            dialogSheet.IsExpanded = false;
            await closed.Task;
            return result;
        }
        finally
        {
            dialogSheet.PropertyChanged -= OnSheetChanged;
            dialogSheet.SheetClosed -= OnSheetClosed;
            dialogSheet.DetachedFromVisualTree -= OnSheetDetached;
            dialogSheet.IsExpanded = false;
            dialogSheet.SheetContent = null;
        }
    }

    private async Task<bool> ShowPageAsync(TView view, DialogCompletion completion)
    {
        var page = new ContentPage
        {
            Content = view,
            Focusable = true
        };

        NavigationPage.SetHasNavigationBar(page, false);

        void OnPopped(object? sender, NavigationEventArgs args)
        {
            if (args.Page != page)
            {
                return;
            }

            if (confirmOnBackNavigation)
            {
                completion.Confirm();
            }
            else
            {
                completion.Cancel();
            }
        }

        void OnKeyDown(object? sender, KeyEventArgs args)
        {
            if (args.Key != Key.Escape || navigationPage.CurrentPage != page)
            {
                return;
            }

            args.Handled = true;
            completion.Cancel();
        }

        navigationPage.Popped += OnPopped;
        page.KeyDown += OnKeyDown;

        try
        {
            await navigationPage.PushAsync(page);
            if (!navigationPage.NavigationStack.Contains(page))
            {
                return false;
            }

            page.Focus();
            var result = await completion.Result;
            await DismissAsync(page);
            return result;
        }
        finally
        {
            navigationPage.Popped -= OnPopped;
            page.KeyDown -= OnKeyDown;
        }
    }

    private async Task DismissAsync(Page page)
    {
        // Navigation ignores pop and remove requests while a transition is in progress.
        while (navigationPage.IsNavigating && navigationPage.NavigationStack.Contains(page))
        {
            var transitionEnded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnNavigationChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
            {
                if (args.Property == NavigationPage.IsNavigatingProperty && !navigationPage.IsNavigating)
                {
                    transitionEnded.TrySetResult();
                }
            }

            navigationPage.PropertyChanged += OnNavigationChanged;
            try
            {
                await transitionEnded.Task;
            }
            finally
            {
                navigationPage.PropertyChanged -= OnNavigationChanged;
            }
        }

        if (navigationPage.CurrentPage == page && navigationPage.CanGoBack)
        {
            await navigationPage.PopAsync();
        }
        else if (navigationPage.NavigationStack.Contains(page))
        {
            navigationPage.RemovePage(page);
        }
    }
}
