using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class GamepadButtonTests(AvaloniaUiThread uiThread)
{
    [Theory]
    [InlineData(0, 0, false, GamepadButtonSelection.Center)]
    [InlineData(-60, 0, false, GamepadButtonSelection.Left)]
    [InlineData(60, 0, false, GamepadButtonSelection.Right)]
    [InlineData(60, -80, false, null)]
    [InlineData(60, 0, true, GamepadButtonSelection.Right)]
    public Task PointerRelease_AtGestureDestination_ReportsOneOutcome(
        double x, double y, bool moveUpBeforeRelease, GamepadButtonSelection? expected) => uiThread.RunAsync(async () =>
    {
        var gamepad = new GamepadButton();
        var thumb = gamepad.FindControl<Button>("ThumbButton")!;
        var window = new Window { Content = gamepad, Width = 300, Height = 260, ShowInTaskbar = false, ShowActivated = false, Opacity = 0 };
        var outcome = new TaskCompletionSource<GamepadButtonSelection?>(TaskCreationOptions.RunContinuationsAsynchronously);
        int reports = 0;
        gamepad.Confirmed += (_, e) =>
        {
            reports++;
            outcome.TrySetResult(e.Selection);
        };
        gamepad.Cancelled += (_, _) =>
        {
            reports++;
            outcome.TrySetResult(null);
        };

        try
        {
            window.Show();
            window.UpdateLayout();
            var pointer = new Pointer(1, PointerType.Mouse, true);
            Point start = thumb.TranslatePoint(new Rect(thumb.Bounds.Size).Center, window)!.Value;
            var press = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            thumb.RaiseEvent(new PointerPressedEventArgs(thumb, pointer, window, start, 0, press, KeyModifiers.None));
            Assert.True(gamepad.IsGestureActive);
            Assert.Same(gamepad, pointer.Captured);

            var destination = start + new Vector(x, y);
            var move = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other);
            if (moveUpBeforeRelease)
            {
                gamepad.RaiseEvent(new PointerEventArgs(InputElement.PointerMovedEvent, gamepad, pointer,
                    window, start + new Vector(0, -80), 1, move, KeyModifiers.None));
            }
            gamepad.RaiseEvent(new PointerEventArgs(InputElement.PointerMovedEvent, gamepad, pointer,
                window, destination, 2, move, KeyModifiers.None));
            var release = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased);
            gamepad.RaiseEvent(new PointerReleasedEventArgs(gamepad, pointer, window, destination, 3, release,
                KeyModifiers.None, MouseButton.Left));

            Assert.Equal(expected, await outcome.Task.WaitAsync(AvaloniaUiThread.Timeout));
            Assert.False(gamepad.IsGestureActive);
            Assert.Null(pointer.Captured);
            Assert.Equal(1, reports);
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public Task ThumbClick_WithCustomBrushes_ConfirmsCenter() => uiThread.RunAsync(() =>
    {
        var gamepad = new GamepadButton();
        var thumb = gamepad.FindControl<Button>("ThumbButton")!;
        var leftChoice = gamepad.FindControl<Border>("LeftChoice")!;
        var rightChoice = gamepad.FindControl<Border>("RightChoice")!;
        Assert.Equal(Color.Parse("#00FA9A"), Assert.IsAssignableFrom<ISolidColorBrush>(gamepad.CenterBrush).Color);
        Assert.Equal(Color.Parse("#F44336"), Assert.IsAssignableFrom<ISolidColorBrush>(gamepad.LeftBrush).Color);
        Assert.Equal(Color.Parse("#0088CC"), Assert.IsAssignableFrom<ISolidColorBrush>(gamepad.RightBrush).Color);

        gamepad.CenterBrush = Brushes.Orange;
        gamepad.LeftBrush = new LinearGradientBrush();
        gamepad.RightBrush = Brushes.Purple;
        AutomationProperties.SetName(gamepad, "Add task");
        Assert.Same(gamepad.CenterBrush, thumb.Background);
        Assert.Same(gamepad.LeftBrush, leftChoice.Background);
        Assert.Same(gamepad.RightBrush, rightChoice.Background);
        Assert.Equal("Add task", AutomationProperties.GetName(thumb));

        var choices = new List<GamepadButtonSelection>();
        gamepad.Confirmed += (_, e) => choices.Add(e.Selection);
        thumb.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(GamepadButtonSelection.Center, Assert.Single(choices));
        return Task.CompletedTask;
    });
}
