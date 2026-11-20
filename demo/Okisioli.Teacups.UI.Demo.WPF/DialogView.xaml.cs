using System.Windows;
using System.Windows.Controls;

namespace Okisioli.Teacups.UI.Demo.WPF;

public partial class DialogView : UserControl
{
    public DialogView()
    {
        InitializeComponent();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs args)
    {
        ((DialogViewModel)DataContext).Confirm();
    }

    private void OnCancelClick(object sender, RoutedEventArgs args)
    {
        ((DialogViewModel)DataContext).Cancel();
    }
}
