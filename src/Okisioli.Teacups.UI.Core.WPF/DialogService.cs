using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Core.WPF;

/// <summary>
/// Shows WPF user controls in a library-owned modal dialog window.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">A <see cref="UserControl"/> with a public parameterless constructor.</typeparam>
/// <param name="dispatcher">The dispatcher for the WPF UI thread.</param>
/// <remarks>
/// The dialog centers on the active application window on the supplied dispatcher, or on the screen
/// when there is no such window. Closing the window or pressing Escape cancels the dialog.
/// Set <see cref="Button.IsDefault"/> on the view's confirmation button to enable Enter.
/// </remarks>
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

            if (Application.Current is { } application && application.CheckAccess()
                && application.Windows.OfType<Window>().FirstOrDefault(candidate => candidate.IsActive) is { } owner)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

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
