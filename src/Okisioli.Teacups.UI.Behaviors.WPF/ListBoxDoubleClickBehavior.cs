using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Invokes a command when a list-box item is double-clicked.</summary>
public sealed class ListBoxDoubleClickBehavior : Behavior<ListBox>
{
    /// <summary>Defines the command invoked for the double-clicked item's data context.</summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ListBoxDoubleClickBehavior));

    /// <summary>Gets or sets the command invoked for the double-clicked item's data context.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
        base.OnDetaching();
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.OriginalSource is not DependencyObject source)
            return;

        var item = FindAncestor<ListBoxItem>(source);
        var parameter = item?.DataContext;

        if (item is null || Command?.CanExecute(parameter) != true)
            return;

        Command.Execute(parameter);
        eventArgs.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
    {
        while (current is not T)
        {
            var parent = VisualTreeHelper.GetParent(current);

            if (parent is null)
                return null;

            current = parent;
        }

        return (T)current;
    }
}
