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

            navigationPage.Popped -= OnPopped;
            if (confirmOnBackNavigation)
            {
                completion.Confirm();
            }
            else
            {
                completion.Cancel();
            }
        }

        async void OnKeyDown(object? sender, KeyEventArgs args)
        {
            if (args.Key != Key.Escape || navigationPage.Navigation?.NavigationStack.Contains(page) != true)
            {
                return;
            }

            args.Handled = true;
            await navigationPage.PopAsync();
        }

        navigationPage.Popped += OnPopped;
        page.KeyDown += OnKeyDown;

        try
        {
            await navigationPage.PushAsync(page);
            page.Focus();
        }
        catch
        {
            navigationPage.Popped -= OnPopped;
            page.KeyDown -= OnKeyDown;
            throw;
        }

        var result = await completion.Result;
        navigationPage.Popped -= OnPopped;
        page.KeyDown -= OnKeyDown;

        if (navigationPage.Navigation?.NavigationStack.Contains(page) == true)
        {
            await navigationPage.PopAsync();
        }

        return result;
    }
}
