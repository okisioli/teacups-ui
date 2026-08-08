using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

public static class LongPressBehavior
{
    private static readonly ConditionalWeakTable<FrameworkElement, PressState> PressStates = new();

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(LongPressBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached(
            "Duration",
            typeof(TimeSpan),
            typeof(LongPressBehavior),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

    public static void SetCommand(FrameworkElement element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(FrameworkElement element)
    {
        return (ICommand?)element.GetValue(CommandProperty);
    }

    public static void SetDuration(FrameworkElement element, TimeSpan value)
    {
        element.SetValue(DurationProperty, value);
    }

    public static TimeSpan GetDuration(FrameworkElement element)
    {
        return (TimeSpan)element.GetValue(DurationProperty);
    }

    private static void OnCommandChanged(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is not FrameworkElement element)
            return;

        var state = PressStates.GetValue(element, _ => new PressState());
        state.Cancel();

        if (eventArgs.OldValue is not null)
        {
            element.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
            element.MouseLeave -= OnMouseLeave;
            element.LostMouseCapture -= OnLostMouseCapture;
        }

        if (eventArgs.NewValue is not null)
        {
            element.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            element.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            element.MouseLeave += OnMouseLeave;
            element.LostMouseCapture += OnLostMouseCapture;
        }
    }

    private static async void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
    {
        if (sender is not FrameworkElement element)
            return;

        var state = PressStates.GetValue(element, _ => new PressState());
        state.Start();

        try
        {
            await Task.Delay(GetDuration(element), state.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        var command = GetCommand(element);
        if (command?.CanExecute(element.DataContext) != true)
            return;

        state.HasInvoked = true;
        command.Execute(element.DataContext);
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
    {
        if (sender is not FrameworkElement element)
            return;

        var state = PressStates.GetValue(element, _ => new PressState());
        state.Cancel();

        if (state.HasInvoked)
            eventArgs.Handled = true;
    }

    private static void OnMouseLeave(object sender, MouseEventArgs eventArgs)
    {
        CancelPress(sender);
    }

    private static void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
    {
        CancelPress(sender);
    }

    private static void CancelPress(object sender)
    {
        if (sender is FrameworkElement element && PressStates.TryGetValue(element, out var state))
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
