using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Invokes a command when a control is pressed for a configured duration.</summary>
public sealed class LongPressBehavior : Behavior<FrameworkElement>
{
    private CancellationTokenSource? cancellationTokenSource;
    private bool hasInvoked;

    /// <summary>Defines the command invoked after a successful long press.</summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(LongPressBehavior));

    /// <summary>Defines the minimum press duration before the command is invoked.</summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan),
            typeof(LongPressBehavior),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

    /// <summary>Gets or sets the command invoked after a successful long press.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets the minimum press duration before the command is invoked.</summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
        AssociatedObject.MouseLeave += OnMouseLeave;
        AssociatedObject.LostMouseCapture += OnLostMouseCapture;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        CancelPress();
        AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        AssociatedObject.MouseLeave -= OnMouseLeave;
        AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
        base.OnDetaching();
    }

    private async void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        CancelPress();
        cancellationTokenSource = new CancellationTokenSource();
        hasInvoked = false;

        try
        {
            await Task.Delay(Duration, cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (Command?.CanExecute(AssociatedObject.DataContext) != true)
            return;

        hasInvoked = true;
        Command.Execute(AssociatedObject.DataContext);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        CancelPress();

        if (hasInvoked)
            eventArgs.Handled = true;
    }

    private void OnMouseLeave(object sender, MouseEventArgs eventArgs)
    {
        CancelPress();
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
    {
        CancelPress();
    }

    private void CancelPress()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }
}
