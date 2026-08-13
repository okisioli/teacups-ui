using Avalonia.Controls;
using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>
/// Shows Avalonia user controls as full-screen pages in a navigation stack.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">The type of the dialog view.</typeparam>
public class DialogService<TViewModel, TView>(NavigationPage navigationPage) : IDialogService<TViewModel, TView>
    where TView : UserControl, new()
{
    /// <inheritdoc />
    public async Task<bool> ShowDialogAsync(TViewModel viewModel)
    {
        var view = new TView
        {
            DataContext = viewModel
        };

        var page = new ContentPage
        {
            Content = view
        };

        NavigationPage.SetHasNavigationBar(page, false);

        var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnPopped(object? sender, NavigationEventArgs args)
        {
            if (args.Page != page)
            {
                return;
            }

            navigationPage.Popped -= OnPopped;
            result.TrySetResult(false);
        }

        navigationPage.Popped += OnPopped;

        try
        {
            await navigationPage.PushAsync(page);
        }
        catch
        {
            navigationPage.Popped -= OnPopped;
            throw;
        }

        return await result.Task;
    }
}
