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
    public async Task<bool> ShowDialogAsync(TViewModel viewModel)
    {
        var operation = dispatcher.InvokeAsync(() =>
        {
            var view = new TView
            {
                DataContext = viewModel
            };

            var window = new DialogWindow
            {
                Content = view
            };

            return window.ShowDialog() ?? false;
        });

        return await operation.Task;
    }
}
