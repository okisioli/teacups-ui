using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// A three-choice button that confirms the center on activation or selects a side by horizontal drag.
/// </summary>
/// <remarks>
/// Swiping up and releasing cancels the gesture. Moving back before releasing resumes selection.
/// </remarks>
public partial class GamepadButton : UserControl
{
    // Gesture tuning: distances are device-independent pixels, threshold is a fraction of side travel.
    private const double SideTravel = 80;
    private const double SelectionThreshold = 0.35;
    private const double SelectionRotationDegrees = 90;
    // Try 0.5 for half speed, 1.0 / 3 for one-third speed, or 0.25 for quarter speed.
    private const double IconRotationSpeedMultiplier = 1.0 / 3;
    private const double UpwardCancelDistance = 72;
    private const double IdleWashOpacity = 0.08;
    private const double BaseWashOpacity = 0.14;
    private const double ExtraWashOpacity = 0.10;
    // Expansion and closing are tuned independently. Quick taps close in proportion to the visible reveal.
    private static readonly TimeSpan RevealDuration = TimeSpan.FromMilliseconds(220);
    private static readonly TimeSpan QuickTapThreshold = TimeSpan.FromMilliseconds(120);
    private static readonly TimeSpan QuickTapMaxClosingDuration = TimeSpan.FromMilliseconds(120);
    private static readonly TimeSpan RetractionDuration = TimeSpan.FromMilliseconds(160);
    // Shared by the opening choices and the closing picker fade.
    private static readonly TimeSpan PickerFadeDuration = TimeSpan.FromMilliseconds(120);
    // How much of the fade runs before retraction ends; clamped to the shorter duration.
    private static readonly TimeSpan PickerFadeOverlap = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan FollowDuration = TimeSpan.FromMilliseconds(60);
    private static readonly TimeSpan ChoiceFadeDuration = TimeSpan.FromMilliseconds(80);
    private static readonly TimeSpan SelectionFadeDuration = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan SelectionSnapDuration = TimeSpan.FromMilliseconds(180);
    private static readonly TimeSpan IconRotationDuration =
        TimeSpan.FromMilliseconds(SelectionSnapDuration.TotalMilliseconds / IconRotationSpeedMultiplier);

    public static readonly StyledProperty<IBrush> CenterBrushProperty =
        AvaloniaProperty.Register<GamepadButton, IBrush>(nameof(CenterBrush), new ImmutableSolidColorBrush(Color.Parse("#00FA9A")));

    public static readonly StyledProperty<IBrush> LeftBrushProperty =
        AvaloniaProperty.Register<GamepadButton, IBrush>(nameof(LeftBrush), new ImmutableSolidColorBrush(Color.Parse("#F44336")));

    public static readonly StyledProperty<IBrush> RightBrushProperty =
        AvaloniaProperty.Register<GamepadButton, IBrush>(nameof(RightBrush), new ImmutableSolidColorBrush(Color.Parse("#0088CC")));

    public static readonly DirectProperty<GamepadButton, bool> IsGestureActiveProperty =
        AvaloniaProperty.RegisterDirect<GamepadButton, bool>(nameof(IsGestureActive), control => control.IsGestureActive);

    private readonly SolidColorBrush wash = new();
    private readonly TranslateTransform leftOffset;
    private readonly TranslateTransform rightOffset;
    private readonly TranslateTransform thumbOffset;
    private readonly TranslateTransform trackOffset;
    private readonly TranslateTransform pickerOffset;
    private readonly RotateTransform iconRotation;
    private IPointer? activePointer;
    private Point pressPosition;
    private GamepadButtonSelection selection;
    private double selectionProgress;
    private long pressTimestamp;
    private long selectionTimestamp;
    private CancellationTokenSource? revealAnimation;
    private CancellationTokenSource? releaseAnimation;
    private bool isGestureActive;

    static GamepadButton()
    {
        ForegroundProperty.OverrideDefaultValue<GamepadButton>(new ImmutableSolidColorBrush(Color.Parse("#181B20")));
    }

    public GamepadButton()
    {
        InitializeComponent();
        leftOffset = (TranslateTransform)LeftChoice.RenderTransform!;
        rightOffset = (TranslateTransform)RightChoice.RenderTransform!;
        iconRotation = (RotateTransform)ThumbIcon.RenderTransform!;
        thumbOffset = (TranslateTransform)ThumbButton.RenderTransform!;
        trackOffset = (TranslateTransform)Track.RenderTransform!;
        pickerOffset = (TranslateTransform)PickerSurface.RenderTransform!;
        TrackFill.Opacity = IdleWashOpacity;
        TrackFill.Background = CenterBrush;
        ThumbButton.Background = CenterBrush;
        wash.Transitions = [new ColorTransition { Property = SolidColorBrush.ColorProperty, Duration = FollowDuration }];
        ThumbButton.Transitions = [new BrushTransition { Property = Button.BackgroundProperty, Duration = SelectionFadeDuration }];
        leftOffset.Transitions = PositionTransitions(RevealDuration);
        rightOffset.Transitions = PositionTransitions(RevealDuration);
        thumbOffset.Transitions = PositionTransitions(SelectionSnapDuration);
        trackOffset.Transitions = PositionTransitions(RevealDuration);
        iconRotation.Transitions =
        [
            new DoubleTransition
            {
                Property = RotateTransform.AngleProperty,
                Duration = IconRotationDuration,
                Easing = new SineEaseInOut()
            }
        ];
        Track.Transitions = [Transition(WidthProperty, RevealDuration)];
        TrackFill.Transitions = [Transition(OpacityProperty, RevealDuration)];

        // Capture before Button handles the press, so touch and mouse share the same drag flow.
        ThumbButton.AddHandler(PointerPressedEvent, OnPress, RoutingStrategies.Tunnel);
        PointerMoved += OnMove;
        PointerReleased += OnRelease;
        PointerCaptureLost += OnCaptureLost;
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        DetachedFromVisualTree += (_, _) => CancelGesture();
    }

    /// <summary>Gets or sets the center choice brush. Defaults to <c>#00FA9A</c>.</summary>
    public IBrush CenterBrush
    {
        get => GetValue(CenterBrushProperty);
        set => SetValue(CenterBrushProperty, value);
    }

    /// <summary>Gets or sets the left choice brush. Defaults to <c>#F44336</c>.</summary>
    public IBrush LeftBrush
    {
        get => GetValue(LeftBrushProperty);
        set => SetValue(LeftBrushProperty, value);
    }

    /// <summary>Gets or sets the right choice brush. Defaults to <c>#0088CC</c>.</summary>
    public IBrush RightBrush
    {
        get => GetValue(RightBrushProperty);
        set => SetValue(RightBrushProperty, value);
    }

    /// <summary>Gets whether a pointer gesture or its closing animation is in progress.</summary>
    public bool IsGestureActive
    {
        get => isGestureActive;
        private set => SetAndRaise(IsGestureActiveProperty, ref isGestureActive, value);
    }

    /// <summary>Occurs when an activation or a completed drag confirms a choice.</summary>
    public event EventHandler<GamepadButtonConfirmedEventArgs>? Confirmed;

    /// <summary>Occurs when an active gesture is cancelled by a swipe, Escape, capture loss, or detachment.</summary>
    public event EventHandler? Cancelled;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != CenterBrushProperty && change.Property != LeftBrushProperty && change.Property != RightBrushProperty)
        {
            return;
        }

        if (ThumbButton is not { } thumb)
        {
            return;
        }

        thumb.Background = ChoiceBrush(selection);
        UpdateTrackBrush();
    }

    private async void OnPress(object? sender, PointerPressedEventArgs e)
    {
        if (activePointer is not null || releaseAnimation is not null)
        {
            e.Handled = true;
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        e.PreventGestureRecognition();
        activePointer = e.Pointer;
        IsGestureActive = true;
        pressTimestamp = Stopwatch.GetTimestamp();
        pressPosition = e.GetPosition(this);
        selection = GamepadButtonSelection.Center;
        selectionTimestamp = 0;
        ThumbButton.Focus(NavigationMethod.Pointer);
        e.Pointer.Capture(this);
        leftOffset.X = -SideTravel;
        rightOffset.X = SideTravel;
        Track.Width = Bounds.Width;
        TrackOutline.IsVisible = true;
        LeftChoice.Transitions = RightChoice.Transitions = null;
        LeftChoice.Opacity = RightChoice.Opacity = 1;
        UpdateSelection(0);
        e.Handled = true;

        using CancellationTokenSource cancellation = new();
        revealAnimation = cancellation;
        PickerChoices.Opacity = 0;
        try
        {
            await AnimateValueAsync(PickerChoices, PickerChoices, OpacityProperty, 1, PickerFadeDuration,
                new LinearEasing(), cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            if (revealAnimation == cancellation)
            {
                revealAnimation = null;
            }
        }
    }

    private void OnMove(object? sender, PointerEventArgs e)
    {
        if (e.Pointer != activePointer)
        {
            return;
        }

        e.PreventGestureRecognition();
        e.Handled = true;
        Point position = e.GetPosition(this);
        if (Stopwatch.GetElapsedTime(pressTimestamp) >= RevealDuration)
        {
            pickerOffset.Y = Math.Min(0, position.Y - pressPosition.Y);
        }

        // Keep cancellation reversible until release so returning to the bar resumes color selection.
        if (!IsUpwardCancellation(position))
        {
            UpdateSelection(position.X - pressPosition.X);
        }
    }

    private async void OnRelease(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Pointer != activePointer)
        {
            return;
        }

        Point position = e.GetPosition(this);
        e.PreventGestureRecognition();
        e.Handled = true;
        bool cancelled = IsUpwardCancellation(position);
        if (!cancelled)
        {
            UpdateSelection(position.X - pressPosition.X);
        }

        GamepadButtonSelection confirmedSelection = selection;
        IPointer pointer = activePointer;
        activePointer = null;
        pointer.Capture(null);
        StopRevealAnimation();
        await FinishGestureAsync(confirmedSelection, cancelled);
    }

    private async Task FinishGestureAsync(GamepadButtonSelection choice, bool cancelled)
    {
        using CancellationTokenSource cancellation = new();
        releaseAnimation = cancellation;
        try
        {
            if (pickerOffset.Y != 0)
            {
                await AnimateValueAsync(PickerSurface, pickerOffset, TranslateTransform.YProperty, 0, RetractionDuration,
                    new CubicEaseOut(), cancellation.Token);
                pickerOffset.Y = 0;
            }

            bool isQuickTap = selectionTimestamp == 0 && Stopwatch.GetElapsedTime(pressTimestamp) < QuickTapThreshold;
            if (!isQuickTap && selectionTimestamp != 0)
            {
                TimeSpan remainingSnap = SelectionSnapDuration - Stopwatch.GetElapsedTime(selectionTimestamp);
                if (remainingSnap > TimeSpan.Zero)
                {
                    await Task.Delay(remainingSnap, cancellation.Token);
                }
            }

            TimeSpan retractionDuration = RetractionDuration;
            TimeSpan fadeDuration = PickerFadeDuration;
            TimeSpan fadeDelay;
            if (isQuickTap)
            {
                double revealProgress = Math.Clamp(Math.Abs(rightOffset.X - leftOffset.X) / (2 * SideTravel), 0, 1);
                retractionDuration = TimeSpan.FromMilliseconds(Math.Min(
                    RetractionDuration.TotalMilliseconds * revealProgress,
                    QuickTapMaxClosingDuration.TotalMilliseconds));
                fadeDuration = TimeSpan.FromTicks(Math.Min(PickerFadeDuration.Ticks, retractionDuration.Ticks));
                fadeDelay = TimeSpan.Zero;
            }
            else
            {
                TimeSpan overlap = TimeSpan.FromTicks(Math.Clamp(
                    PickerFadeOverlap.Ticks, 0, Math.Min(retractionDuration.Ticks, fadeDuration.Ticks)));
                fadeDelay = retractionDuration - overlap;
            }

            double destination = SelectionOffset(choice);
            LeftChoice.Transitions = [Transition(OpacityProperty, ChoiceFadeDuration)];
            RightChoice.Transitions = [Transition(OpacityProperty, ChoiceFadeDuration)];
            LeftChoice.Opacity = choice == GamepadButtonSelection.Left ? 1 : 0;
            RightChoice.Opacity = choice == GamepadButtonSelection.Right ? 1 : 0;
            // Keep the picker until both retraction and fade have actually finished.
            await Task.WhenAll(
                AnimateValueAsync(LeftChoice, leftOffset, TranslateTransform.XProperty, destination, retractionDuration,
                    new CubicEaseOut(), cancellation.Token),
                AnimateValueAsync(RightChoice, rightOffset, TranslateTransform.XProperty, destination, retractionDuration,
                    new CubicEaseOut(), cancellation.Token),
                AnimateValueAsync(Track, trackOffset, TranslateTransform.XProperty, destination, retractionDuration,
                    new CubicEaseOut(), cancellation.Token),
                AnimateValueAsync(Track, Track, WidthProperty, Track.Height, retractionDuration,
                    new CubicEaseOut(), cancellation.Token),
                AnimateValueAsync(PickerSurface, PickerSurface, OpacityProperty, 0, fadeDuration,
                    new LinearEasing(), cancellation.Token, fadeDelay));
            cancellation.Token.ThrowIfCancellationRequested();
            releaseAnimation = null;
            ResetGesture();
            if (cancelled)
            {
                Cancelled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Confirmed?.Invoke(this, new GamepadButtonConfirmedEventArgs(choice));
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            if (releaseAnimation == cancellation)
            {
                releaseAnimation = null;
            }
        }
    }

    private static async Task AnimateValueAsync(Visual target, Animatable valueTarget, StyledProperty<double> property,
        double destination, TimeSpan duration, Easing easing, CancellationToken cancellationToken,
        TimeSpan delay = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Capture the displayed value before removing a reveal transition, which exposes its final target.
        double current = valueTarget.GetValue(property);
        Transitions? transitions = valueTarget.Transitions;
        valueTarget.Transitions = null;
        valueTarget.SetValue(property, current);
        try
        {
            if (duration == TimeSpan.Zero && delay == TimeSpan.Zero)
            {
                valueTarget.SetValue(property, destination);
                return;
            }

            Animation animation = new()
            {
                Duration = duration,
                Delay = delay,
                Easing = easing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(property, current) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(property, destination) } }
                }
            };
            // Avalonia resolves transform setters through the owning visual's RenderTransform.
            await animation.RunAsync(target, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        finally
        {
            valueTarget.Transitions = transitions;
        }
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (e.Pointer == activePointer)
        {
            CancelGesture();
        }
    }

    private bool IsUpwardCancellation(Point position) => pressPosition.Y - position.Y >= UpwardCancelDistance;

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsGestureActive)
        {
            e.Handled = true;
            CancelGesture();
        }
    }

    private void OnThumbClick(object? sender, RoutedEventArgs e)
    {
        if (!IsGestureActive)
        {
            Confirmed?.Invoke(this, new GamepadButtonConfirmedEventArgs(GamepadButtonSelection.Center));
        }
    }

    private void UpdateSelection(double distance)
    {
        double progress = selectionProgress = Math.Clamp(distance / SideTravel, -1, 1);
        GamepadButtonSelection nextSelection = progress <= -SelectionThreshold ? GamepadButtonSelection.Left
            : progress >= SelectionThreshold ? GamepadButtonSelection.Right
            : GamepadButtonSelection.Center;

        if (nextSelection != selection)
        {
            selection = nextSelection;
            selectionTimestamp = Stopwatch.GetTimestamp();
            double offset = SelectionOffset(selection);
            thumbOffset.X = offset;
            iconRotation.Angle = Math.Sign(offset) * SelectionRotationDegrees;
            ThumbButton.Background = ChoiceBrush(selection);
        }

        UpdateTrackBrush();
        TrackFill.Opacity = BaseWashOpacity + ExtraWashOpacity * Math.Abs(progress);
    }

    private void UpdateTrackBrush()
    {
        if (!IsGestureActive)
        {
            TrackFill.Background = CenterBrush;
            return;
        }

        IBrush sideBrush = selectionProgress < 0 ? LeftBrush : RightBrush;
        if (CenterBrush is ISolidColorBrush center && sideBrush is ISolidColorBrush side)
        {
            wash.Color = Blend(center.Color, side.Color, Math.Abs(selectionProgress));
            TrackFill.Background = TrackOutline.BorderBrush = wash;
        }
        else
        {
            TrackFill.Background = TrackOutline.BorderBrush = ChoiceBrush(selection);
        }
    }

    private void CancelGesture()
    {
        if (IsGestureActive)
        {
            ResetGesture();
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ResetGesture()
    {
        StopRevealAnimation();
        releaseAnimation?.Cancel();
        releaseAnimation = null;
        IPointer? pointer = activePointer;
        activePointer = null;
        pointer?.Capture(null);
        leftOffset.X = rightOffset.X = leftOffset.Y = rightOffset.Y = thumbOffset.X = trackOffset.X = 0;
        pickerOffset.Y = 0;
        iconRotation.Angle = 0;
        LeftChoice.Opacity = RightChoice.Opacity = 0;
        TrackFill.Opacity = IdleWashOpacity;
        TrackFill.Background = CenterBrush;
        Track.Width = Track.Height;
        TrackOutline.IsVisible = false;
        PickerChoices.Opacity = 1;
        PickerSurface.Opacity = 1;
        ThumbButton.Background = CenterBrush;
        selection = GamepadButtonSelection.Center;
        selectionProgress = 0;
        IsGestureActive = false;
    }

    private void StopRevealAnimation()
    {
        if (revealAnimation is not { } cancellation)
        {
            return;
        }

        // Preserve the visible fade progress when a release reverses a partly opened picker.
        double opacity = PickerChoices.Opacity;
        PickerChoices.Opacity = opacity;
        revealAnimation = null;
        cancellation.Cancel();
    }

    private IBrush ChoiceBrush(GamepadButtonSelection choice) => choice switch
    {
        GamepadButtonSelection.Left => LeftBrush,
        GamepadButtonSelection.Right => RightBrush,
        _ => CenterBrush
    };

    private static double SelectionOffset(GamepadButtonSelection choice) => choice switch
    {
        GamepadButtonSelection.Left => -SideTravel,
        GamepadButtonSelection.Right => SideTravel,
        _ => 0
    };

    private static Color Blend(Color from, Color to, double amount) => Color.FromRgb(
        (byte)Math.Round(from.R + (to.R - from.R) * amount),
        (byte)Math.Round(from.G + (to.G - from.G) * amount),
        (byte)Math.Round(from.B + (to.B - from.B) * amount));

    private static Transitions PositionTransitions(TimeSpan duration) =>
    [
        Transition(TranslateTransform.XProperty, duration),
        Transition(TranslateTransform.YProperty, duration)
    ];

    private static DoubleTransition Transition(AvaloniaProperty property, TimeSpan duration) => new()
    {
        Property = property,
        Duration = duration,
        Easing = new CubicEaseOut()
    };
}
