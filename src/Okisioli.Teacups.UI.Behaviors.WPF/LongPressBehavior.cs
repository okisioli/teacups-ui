using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Runs a command after the user presses and holds the associated control.</summary>
public sealed class LongPressBehavior : Behavior<FrameworkElement>
{
    private CancellationTokenSource? cancellationTokenSource;
    private bool hasInvoked;

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(LongPressBehavior));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan),
            typeof(LongPressBehavior),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

    /// <summary>Gets or sets the command, which receives the associated control's data context.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets how long to hold before invoking the command. Defaults to 500 milliseconds.</summary>
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
