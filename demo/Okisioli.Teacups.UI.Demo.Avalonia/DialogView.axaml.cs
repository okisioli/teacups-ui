using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class DialogView : UserControl
{
    public DialogView()
    {
        InitializeComponent();
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs args)
    {
        ((DialogViewModel)DataContext!).Confirm();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs args)
    {
        ((DialogViewModel)DataContext!).Cancel();
    }
}
