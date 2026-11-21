using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>
/// Shows Avalonia user controls as full-screen pages in a navigation stack.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">A <see cref="UserControl"/> with a public parameterless constructor.</typeparam>
/// <param name="navigationPage">The navigation page that hosts the dialog.</param>
/// <param name="confirmOnBackNavigation">
/// <see langword="true"/> to confirm when back navigation pops the dialog page.
/// <see langword="false"/> to cancel.
/// </param>
/// <remarks>
/// Escape cancels the dialog. Set <see cref="Button.IsDefault"/> on the view's confirmation button
/// to enable Enter.
/// </remarks>
public class DialogService<TViewModel, TView>(NavigationPage navigationPage, bool confirmOnBackNavigation) : IDialogService<TViewModel, TView>
    where TView : UserControl, new()
{
    /// <inheritdoc />
    public Task<bool> ShowDialogAsync(TViewModel viewModel)
    {
        return ShowDialogAsync(_ => viewModel);
    }

    /// <inheritdoc />
    public async Task<bool> ShowDialogAsync(Func<IDialogCompletion, TViewModel> viewModelFactory)
    {
        var completion = new DialogCompletion();
        var viewModel = viewModelFactory(completion);
        var view = new TView
        {
            DataContext = viewModel
        };

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
