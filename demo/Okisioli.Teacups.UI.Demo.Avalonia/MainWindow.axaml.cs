using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Okisioli.Teacups.UI.Core;
using System.Windows.Input;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class MainWindow : UserControl
{
    private readonly IDialogService<DialogViewModel, DialogView>? dialogService;
    private readonly IPopupService? popupService;

    public MainWindow()
        : this(null, null)
    {
    }

    public MainWindow(IDialogService<DialogViewModel, DialogView>? dialogService, IPopupService? popupService)
    {
        this.dialogService = dialogService;
        this.popupService = popupService;
        DemoItems = ["First item", "Second item", "Third item"];
        LongPressCommand = new DelegateCommand(_ => LongPressResult.Text = "Long press detected.");
        ListBoxDoubleClickCommand = new DelegateCommand(item => ListBoxDoubleClickResult.Text = $"Double-clicked: {item}");
        TextBoxSubmitCommand = new DelegateCommand(text => TextBoxSubmitResult.Text = $"Submitted: \"{text}\"");
        TagSubmitCommand = new DelegateCommand(tag => TagSubmitResult.Text = $"Last tag: \"{tag}\"");

        DataContext = this;
        InitializeComponent();
    }

    public IReadOnlyList<string> DemoItems { get; }

    public IReadOnlyList<Dock> DrawerPlacements { get; } = [Dock.Bottom, Dock.Left, Dock.Top, Dock.Right];
    public IReadOnlyList<HorizontalAlignment> DrawerHorizontalAlignments { get; } =
        [HorizontalAlignment.Stretch, HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right];
    public IReadOnlyList<VerticalAlignment> DrawerVerticalAlignments { get; } =
        [VerticalAlignment.Stretch, VerticalAlignment.Top, VerticalAlignment.Center, VerticalAlignment.Bottom];
    public ICommand LongPressCommand { get; }

    public ICommand ListBoxDoubleClickCommand { get; }

    public ICommand TextBoxSubmitCommand { get; }

    public ICommand TagSubmitCommand { get; }

    public bool CanShowDialog => dialogService is not null;

    public bool CanShowPopup => popupService is not null;

    public string DialogResult { get; private set; } = "No dialog result yet.";

    private void OnShowDrawerSheetClick(object? sender, RoutedEventArgs args)
    {
        if (DrawerPlacement.SelectedItem is Dock placement)
        {
            DemoDrawerSheet.Placement = placement;
        }

        DemoDrawerSheet.SheetHorizontalAlignment =
            DrawerHorizontalAlignment.SelectedItem is HorizontalAlignment horizontal ? horizontal : HorizontalAlignment.Stretch;
        DemoDrawerSheet.SheetVerticalAlignment =
            DrawerVerticalAlignment.SelectedItem is VerticalAlignment vertical ? vertical : VerticalAlignment.Stretch;
        DemoDrawerSheet.SheetMinWidth = (double)(DrawerMinWidth.Value ?? 0);
        DemoDrawerSheet.SheetMinHeight = (double)(DrawerMinHeight.Value ?? 0);
        DemoDrawerSheet.IsExpanded = true;
    }

    private void OnCloseDrawerSheetClick(object? sender, RoutedEventArgs args)
    {
        DemoDrawerSheet.IsExpanded = false;
    }

    private async void OnShowDialogClick(object? sender, RoutedEventArgs args)
    {
        if (dialogService is null)
        {
            return;
        }

        dialogService.FullScreen = FullScreenDialog.IsChecked == true;
        var accepted = await dialogService.ShowDialogAsync(completion => new DialogViewModel(completion));
        DialogResult = accepted ? "Dialog accepted." : "Dialog cancelled.";
        DataContext = null;
        DataContext = this;
    }

    private void OnShowToastClick(object? sender, RoutedEventArgs args)
    {
        popupService?.ShowToast("Settings saved.");
    }

    private void OnShowSnackbarClick(object? sender, RoutedEventArgs args)
    {
        if (popupService is { } service)
        {
            service.ShowSnackbar("Item removed.", "Undo", () => service.ShowToast("Item restored."));
        }
    }

    private sealed class DelegateCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute(parameter);
    }
}
