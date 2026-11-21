using Avalonia.Controls;
using Avalonia.Interactivity;
using Okisioli.Teacups.UI.Core;
using System.Windows.Input;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class MainWindow : UserControl
{
    private readonly IDialogService<DialogViewModel, DialogView> dialogService;

    public MainWindow(IDialogService<DialogViewModel, DialogView> dialogService)
    {
        this.dialogService = dialogService;
        DemoItems = ["First item", "Second item", "Third item"];
        LongPressCommand = new DelegateCommand(_ => LongPressResult.Text = "Long press detected.");
        ListBoxDoubleClickCommand = new DelegateCommand(item => ListBoxDoubleClickResult.Text = $"Double-clicked: {item}");
        TextBoxSubmitCommand = new DelegateCommand(text => TextBoxSubmitResult.Text = $"Submitted: \"{text}\"");
        TagSubmitCommand = new DelegateCommand(tag => TagSubmitResult.Text = $"Last tag: \"{tag}\"");

        DataContext = this;
        InitializeComponent();
    }

    public IReadOnlyList<string> DemoItems { get; }

    public ICommand LongPressCommand { get; }

    public ICommand ListBoxDoubleClickCommand { get; }

    public ICommand TextBoxSubmitCommand { get; }

    public ICommand TagSubmitCommand { get; }

    public string DialogResult { get; private set; } = "No dialog result yet.";

    private async void OnShowDialogClick(object? sender, RoutedEventArgs args)
    {
        var accepted = await dialogService.ShowDialogAsync(completion => new DialogViewModel(completion));
        DialogResult = accepted ? "Dialog accepted." : "Dialog cancelled.";
        DataContext = null;
        DataContext = this;
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
