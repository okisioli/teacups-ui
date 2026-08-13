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
            Content = view
        };

        NavigationPage.SetHasNavigationBar(page, false);

        void OnPopped(object? sender, NavigationEventArgs args)
        {
            if (args.Page != page)
            {
                return;
            }

            navigationPage.Popped -= OnPopped;
            completion.Cancel();
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

        var result = await completion.Result;
        navigationPage.Popped -= OnPopped;

        if (result && navigationPage.Navigation?.NavigationStack.Contains(page) == true)
        {
            await navigationPage.PopAsync();
        }

        return result;
    }
}
