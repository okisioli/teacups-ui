using System.Globalization;
using Avalonia.Controls;

namespace Okisioli.Teacups.UI.Core.Avalonia;

internal partial class PopupView : UserControl
{
    public PopupView()
    {
        InitializeComponent();
    }

    public PopupView(string text) : this()
    {
        Message.Text = text;
    }

    public PopupView(string text, string actionText, Action action) : this(text)
    {
        Classes.Add("snackbar");
        ActionButton.Content = actionText.ToUpper(CultureInfo.CurrentUICulture);
        ActionButton.IsVisible = true;
        ActionButton.Click += (_, _) => action();
    }
}
