using System.Windows;
using System.Windows.Input;

namespace Okisioli.Teacups.UI.Core.WPF;

internal sealed class DialogWindow : Window
{
    public DialogWindow()
    {
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Escape)
        {
            args.Handled = true;
            Close();
        }
    }
}
