using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// Hosts an application's main view with separate snackbar and toast layers above it.
/// </summary>
/// <remarks>Use <see cref="MainContent"/> for the application view or the drawer sheet that hosts its dialogs.</remarks>
public partial class RootView : UserControl
{
    /// <summary>Defines the application content property.</summary>
    public static readonly StyledProperty<Control?> MainContentProperty =
        AvaloniaProperty.Register<RootView, Control?>(nameof(MainContent));

    /// <summary>Defines the snackbar layer content property.</summary>
    public static readonly StyledProperty<Control?> SnackbarContentProperty =
        AvaloniaProperty.Register<RootView, Control?>(nameof(SnackbarContent));

    /// <summary>Defines the toast layer content property.</summary>
    public static readonly StyledProperty<Control?> ToastContentProperty =
        AvaloniaProperty.Register<RootView, Control?>(nameof(ToastContent));

    /// <summary>Initializes an empty root view.</summary>
    public RootView()
    {
        InitializeComponent();
    }

    /// <summary>Gets or sets the application view beneath the popup layers.</summary>
    public Control? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    /// <summary>Gets or sets the content of the interactive snackbar layer.</summary>
    public Control? SnackbarContent
    {
        get => GetValue(SnackbarContentProperty);
        set => SetValue(SnackbarContentProperty, value);
    }

    /// <summary>Gets or sets the content of the passive, topmost toast layer.</summary>
    public Control? ToastContent
    {
        get => GetValue(ToastContentProperty);
        set => SetValue(ToastContentProperty, value);
    }

    internal bool IsInSnackbar(Visual source) => SnackbarLayer.IsVisualAncestorOf(source);
}
