using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

/// <summary>Invokes a command when a control is pressed for a configured duration.</summary>
public sealed class LongPressBehavior : Behavior<Control>
{
    private CancellationTokenSource? cancellationTokenSource;
    private bool hasInvoked;

    /// <summary>Defines the command invoked after a successful long press.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<LongPressBehavior, ICommand?>(nameof(Command));

    /// <summary>Defines the minimum press duration before the command is invoked.</summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<LongPressBehavior, TimeSpan>(
            nameof(Duration),
            TimeSpan.FromMilliseconds(500),
            validate: duration => duration >= TimeSpan.Zero);

    /// <summary>Gets or sets the command invoked after a successful long press.</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets the minimum press duration before the command is invoked.</summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject!.AddHandler(
            InputElement.PointerPressedEvent,
            OnPointerPressed,
            RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(
            InputElement.PointerReleasedEvent,
            OnPointerReleased,
            RoutingStrategies.Tunnel);
        AssociatedObject.PointerExited += OnPointerExited;
        AssociatedObject.PointerCaptureLost += OnPointerCaptureLost;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        CancelPress();
        AssociatedObject!.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        AssociatedObject.PointerExited -= OnPointerExited;
        AssociatedObject.PointerCaptureLost -= OnPointerCaptureLost;
        base.OnDetaching();
    }

    private async void OnPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
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

        if (AssociatedObject is not { } associatedObject
            || Command?.CanExecute(associatedObject.DataContext) != true)
            return;

        hasInvoked = true;
        Command.Execute(associatedObject.DataContext);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        CancelPress();

        if (hasInvoked)
            eventArgs.Handled = true;
    }

    private void OnPointerExited(object? sender, PointerEventArgs eventArgs)
    {
        CancelPress();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs eventArgs)
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
