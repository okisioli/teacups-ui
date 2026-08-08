using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

public sealed class LongPressBehavior
{
    private static readonly ConditionalWeakTable<Control, PressState> PressStates = new();

    public static readonly AttachedProperty<ICommand?> CommandProperty =
        AvaloniaProperty.RegisterAttached<LongPressBehavior, Control, ICommand?>("Command");

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<LongPressBehavior, Control, TimeSpan>(
            "Duration",
            TimeSpan.FromMilliseconds(500),
            validate: duration => duration >= TimeSpan.Zero);

    static LongPressBehavior()
    {
        CommandProperty.Changed.AddClassHandler<Control>(OnCommandChanged);
    }

    private LongPressBehavior()
    {
    }

    public static void SetCommand(Control element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(Control element)
    {
        return element.GetValue(CommandProperty);
    }

    public static void SetDuration(Control element, TimeSpan value)
    {
        element.SetValue(DurationProperty, value);
    }

    public static TimeSpan GetDuration(Control element)
    {
        return element.GetValue(DurationProperty);
    }

    private static void OnCommandChanged(Control control, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        var state = PressStates.GetValue(control, _ => new PressState());
        state.Cancel();

        if (eventArgs.OldValue is not null)
        {
            control.PointerPressed -= OnPointerPressed;
            control.PointerReleased -= OnPointerReleased;
            control.PointerExited -= OnPointerExited;
            control.PointerCaptureLost -= OnPointerCaptureLost;
        }

        if (eventArgs.NewValue is not null)
        {
            control.PointerPressed += OnPointerPressed;
            control.PointerReleased += OnPointerReleased;
            control.PointerExited += OnPointerExited;
            control.PointerCaptureLost += OnPointerCaptureLost;
        }
    }

    private static async void OnPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (sender is not Control control)
            return;

        var state = PressStates.GetValue(control, _ => new PressState());
        state.Start();

        try
        {
            await Task.Delay(GetDuration(control), state.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        var command = GetCommand(control);
        if (command?.CanExecute(control.DataContext) != true)
            return;

        state.HasInvoked = true;
        command.Execute(control.DataContext);
    }

    private static void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        if (sender is not Control control)
            return;

        var state = PressStates.GetValue(control, _ => new PressState());
        state.Cancel();

        if (state.HasInvoked)
            eventArgs.Handled = true;
    }

    private static void OnPointerExited(object? sender, PointerEventArgs eventArgs)
    {
        CancelPress(sender);
    }

    private static void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs eventArgs)
    {
        CancelPress(sender);
    }

    private static void CancelPress(object? sender)
    {
        if (sender is Control control && PressStates.TryGetValue(control, out var state))
            state.Cancel();
    }

    private sealed class PressState
    {
        private CancellationTokenSource? cancellationTokenSource;

        public CancellationToken CancellationToken => cancellationTokenSource?.Token ?? CancellationToken.None;

        public bool HasInvoked { get; set; }

        public void Start()
        {
            Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            HasInvoked = false;
        }

        public void Cancel()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}
