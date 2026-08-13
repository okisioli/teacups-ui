using System.Windows;

namespace Okisioli.Teacups.UI.Core.WPF;

internal sealed class DialogWindow : Window
{
    public DialogWindow()
    {
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}
