using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

public sealed class ListBoxDoubleClickBehavior : Behavior<ListBox>
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ListBoxDoubleClickBehavior, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject!.DoubleTapped += OnDoubleTapped;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.DoubleTapped -= OnDoubleTapped;
        base.OnDetaching();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs eventArgs)
    {
        if (eventArgs.Source is not Visual source)
            return;

        var item = FindAncestor<ListBoxItem>(source);
        var parameter = item?.DataContext;

        if (item is null || Command?.CanExecute(parameter) != true)
            return;

        Command.Execute(parameter);
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
