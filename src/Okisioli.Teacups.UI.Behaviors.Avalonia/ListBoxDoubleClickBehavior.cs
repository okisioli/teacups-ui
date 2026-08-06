using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

public sealed class ListBoxDoubleClickBehavior
{
    public static readonly AttachedProperty<ICommand?> CommandProperty =
        AvaloniaProperty.RegisterAttached<ListBoxDoubleClickBehavior, ListBox, ICommand?>("Command");

    static ListBoxDoubleClickBehavior()
    {
        CommandProperty.Changed.AddClassHandler<ListBox>(OnCommandChanged);
    }

    private ListBoxDoubleClickBehavior()
    {
    }

    public static void SetCommand(ListBox element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(ListBox element)
    {
        return element.GetValue(CommandProperty);
    }

    private static void OnCommandChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.OldValue is not null)
            listBox.DoubleTapped -= OnDoubleTapped;

        if (eventArgs.NewValue is not null)
            listBox.DoubleTapped += OnDoubleTapped;
    }

    private static void OnDoubleTapped(object? sender, TappedEventArgs eventArgs)
    {
        if (sender is not ListBox listBox || eventArgs.Source is not Visual source)
            return;

        var item = FindAncestor<ListBoxItem>(source);

        if (item is null)
            return;

        var command = GetCommand(listBox);
        var parameter = item.DataContext;

        if (command?.CanExecute(parameter) != true)
            return;

        command.Execute(parameter);
        eventArgs.Handled = true;
    }

    private static T? FindAncestor<T>(Visual current)
        where T : Visual
    {
        while (current is not T)
        {
            var parent = current.GetVisualParent();

            if (parent is null)
                return null;

            current = parent;
        }

        return (T)current;
    }
}
