using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Core.WPF;

/// <summary>
/// Shows WPF user controls in a library-owned modal dialog window.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">The type of the dialog view.</typeparam>
public class DialogService<TViewModel, TView>(Dispatcher dispatcher) : IDialogService<TViewModel, TView>
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
        var operation = dispatcher.InvokeAsync(() =>
        {
            var viewModel = viewModelFactory(completion);
            var view = new TView
            {
                DataContext = viewModel
            };

            var window = new DialogWindow
            {
                Content = view
            };

            window.Closed += (_, _) => completion.Cancel();
            _ = completion.Result.ContinueWith(
                _ => dispatcher.BeginInvoke(() =>
                {
                    if (window.IsVisible)
                    {
                        window.Close();
                    }
                }),
                TaskScheduler.Default);

            window.ShowDialog();
        });

        await operation.Task;
        return await completion.Result;
    }
}
