using Avalonia.Controls;
using System.Windows.Input;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DemoItems = ["First item", "Second item", "Third item"];
        LongPressCommand = new DelegateCommand(_ => Console.WriteLine("Long press behavior executed."));
        ListBoxDoubleClickCommand = new DelegateCommand(item => Console.WriteLine($"ListBox double-click behavior executed for: {item}."));

        DataContext = this;
        InitializeComponent();
    }

    public IReadOnlyList<string> DemoItems { get; }

    public ICommand LongPressCommand { get; }

    public ICommand ListBoxDoubleClickCommand { get; }

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
