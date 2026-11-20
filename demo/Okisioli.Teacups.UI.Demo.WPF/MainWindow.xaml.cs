using System.Windows;
using Okisioli.Teacups.UI.Core;
using Okisioli.Teacups.UI.Core.WPF;

namespace Okisioli.Teacups.UI.Demo.WPF;

public partial class MainWindow : Window
{
    private readonly IDialogService<DialogViewModel, DialogView> dialogService;

    public MainWindow()
    {
        InitializeComponent();
        dialogService = new DialogService<DialogViewModel, DialogView>(Dispatcher);
    }

    private async void OnShowDialogClick(object sender, RoutedEventArgs args)
    {
        var accepted = await dialogService.ShowDialogAsync(completion => new DialogViewModel(completion));
        DialogResultText.Text = accepted ? "Dialog accepted." : "Dialog cancelled.";
    }
}
