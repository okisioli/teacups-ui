using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// Displays content with an optional scrollable sheet that slides in from a configurable edge.
/// </summary>
public partial class DrawerSheet : UserControl
{
    private static readonly ConditionalWeakTable<TopLevel, List<DrawerSheet>> OpenSheets = new();

    /// <summary>
    /// Defines the content displayed in the sheet.
    /// </summary>
    public static readonly StyledProperty<object?> SheetContentProperty =
        AvaloniaProperty.Register<DrawerSheet, object?>(nameof(SheetContent));

    /// <summary>
    /// Defines the horizontal alignment of the sheet.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> SheetHorizontalAlignmentProperty =
        AvaloniaProperty.Register<DrawerSheet, HorizontalAlignment>(
            nameof(SheetHorizontalAlignment), defaultValue: HorizontalAlignment.Stretch);

    /// <summary>
    /// Defines the vertical alignment of the sheet.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> SheetVerticalAlignmentProperty =
        AvaloniaProperty.Register<DrawerSheet, VerticalAlignment>(
            nameof(SheetVerticalAlignment), defaultValue: VerticalAlignment.Stretch);

    /// <summary>
    /// Defines the minimum width of the sheet content.
    /// </summary>
    public static readonly StyledProperty<double> SheetMinWidthProperty =
        AvaloniaProperty.Register<DrawerSheet, double>(
            nameof(SheetMinWidth), validate: value => double.IsFinite(value) && value >= 0);

    /// <summary>
    /// Defines the minimum height of the sheet content.
    /// </summary>
    public static readonly StyledProperty<double> SheetMinHeightProperty =
        AvaloniaProperty.Register<DrawerSheet, double>(
            nameof(SheetMinHeight), validate: value => double.IsFinite(value) && value >= 0);

    /// <summary>
    /// Defines whether the sheet is visible.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<DrawerSheet, bool>(nameof(IsExpanded), defaultValue: false);

    /// <summary>
    /// Defines whether opening the sheet resets its scroll position.
    /// </summary>
    public static readonly StyledProperty<bool> ResetScrollPositionProperty =
        AvaloniaProperty.Register<DrawerSheet, bool>(nameof(ResetScrollPosition), defaultValue: true);

    /// <summary>
    /// Defines whether the sheet stays open when Escape, system Back, or an outside press occurs.
    /// </summary>
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<DrawerSheet, bool>(nameof(IsModal), defaultValue: false);

    /// <summary>
    /// Defines the transition played when the sheet opens.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> OpenTransitionProperty =
        AvaloniaProperty.Register<DrawerSheet, IPageTransition?>(nameof(OpenTransition));

    /// <summary>
    /// Defines the transition played when the sheet closes.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> CloseTransitionProperty =
        AvaloniaProperty.Register<DrawerSheet, IPageTransition?>(nameof(CloseTransition));

    /// <summary>
    /// Defines the speed multiplier for the built-in opening and closing transitions.
    /// </summary>
    public static readonly StyledProperty<double> TransitionSpeedProperty =
        AvaloniaProperty.Register<DrawerSheet, double>(
            nameof(TransitionSpeed), defaultValue: 1.5, validate: value => double.IsFinite(value) && value > 0);

    /// <summary>
    /// Defines the brush covering the main content when the sheet is open.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackdropBrushProperty =
        AvaloniaProperty.Register<DrawerSheet, IBrush?>(nameof(BackdropBrush));

    /// <summary>
    /// Defines the edge from which the sheet opens.
    /// </summary>
    public static readonly StyledProperty<Dock> PlacementProperty =
        AvaloniaProperty.Register<DrawerSheet, Dock>(
            nameof(Placement), defaultValue: Dock.Bottom,
            validate: value => value is Dock.Bottom or Dock.Left or Dock.Top or Dock.Right);

    private object? mainContent;
    private object? layoutContent;
    private ContentPresenter? mainContentPresenter;
    private TopLevel? topLevel;
    private Page? page;
    private CancellationTokenSource? activeTransition;
    private bool isInitializingLayout;
    private bool isRestoringLayout;

    /// <summary>Occurs when the sheet is hidden after closing.</summary>
    public event EventHandler? SheetClosed;

    public DrawerSheet()
    {
        isInitializingLayout = true;
        InitializeComponent();
        isInitializingLayout = false;

        layoutContent = base.Content;
        mainContentPresenter = this.FindControl<ContentPresenter>("MainContentPresenter");
    }

    /// <summary>
    /// Gets or sets the content displayed behind the sheet.
    /// </summary>
    [Content]
    public new object? Content
    {
        get => mainContent;
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed in the sheet.
    /// </summary>
    public object? SheetContent
    {
        get => GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the sheet's horizontal alignment within the overlay. Defaults to Stretch.
    /// </summary>
    public HorizontalAlignment SheetHorizontalAlignment
    {
        get => GetValue(SheetHorizontalAlignmentProperty);
        set => SetValue(SheetHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the sheet's vertical alignment within the overlay. Defaults to Stretch.
    /// </summary>
    public VerticalAlignment SheetVerticalAlignment
    {
        get => GetValue(SheetVerticalAlignmentProperty);
        set => SetValue(SheetVerticalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum sheet width in device-independent pixels. Defaults to zero.
    /// The sheet content can scroll if the minimum exceeds the available width.
    /// </summary>
    public double SheetMinWidth
    {
        get => GetValue(SheetMinWidthProperty);
        set => SetValue(SheetMinWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum sheet height in device-independent pixels. Defaults to zero.
    /// The sheet content can scroll if the minimum exceeds the available height.
    /// </summary>
    public double SheetMinHeight
    {
        get => GetValue(SheetMinHeightProperty);
        set => SetValue(SheetMinHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge from which the sheet opens and toward which it closes.
    /// Defaults to <see cref="Dock.Bottom"/>.
    /// </summary>
    public Dock Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the sheet is visible.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether opening the sheet scrolls its content back to the start on both axes.
    /// Defaults to <see langword="true"/>. Set to <see langword="false"/> to retain the previous position.
    /// </summary>
    public bool ResetScrollPosition
    {
        get => GetValue(ResetScrollPositionProperty);
        set => SetValue(ResetScrollPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the sheet stays open when Escape, system Back, or an outside press occurs.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    /// <summary>
    /// Gets or sets the opening transition. Defaults to a slide from <see cref="Placement"/>. Set to
    /// <see langword="null"/> to show the sheet immediately.
    /// </summary>
    /// <remarks>A custom transition receives the sheet as the incoming visual.</remarks>
    public IPageTransition? OpenTransition
    {
        get => GetValue(OpenTransitionProperty);
        set => SetValue(OpenTransitionProperty, value);
    }

    /// <summary>
    /// Gets or sets the closing transition. Defaults to a slide toward <see cref="Placement"/>. Set to
    /// <see langword="null"/> to hide the sheet immediately.
    /// </summary>
    /// <remarks>A custom transition receives the sheet as the outgoing visual with backward direction.</remarks>
    public IPageTransition? CloseTransition
    {
        get => GetValue(CloseTransitionProperty);
        set => SetValue(CloseTransitionProperty, value);
    }

    /// <summary>
    /// Gets or sets the positive, finite speed multiplier for both built-in slides.
    /// Defaults to 1.5, which plays each slide in about 167 milliseconds.
    /// </summary>
    /// <remarks>Custom transitions control their own timing.</remarks>
    public double TransitionSpeed
    {
        get => GetValue(TransitionSpeedProperty);
        set => SetValue(TransitionSpeedProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush that fades in behind the sheet as it opens.
    /// Defaults to 50% black. Set to <see langword="null"/> to hide the backdrop.
    /// </summary>
    public IBrush? BackdropBrush
    {
        get => GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is { } host)
        {
            host.AddHandler(KeyDownEvent, OnHostKeyDown, RoutingStrategies.Tunnel);
            host.AddHandler(PointerPressedEvent, OnHostPointerPressed, RoutingStrategies.Tunnel);
            host.BackRequested += OnHostBackRequested;
        }

        // A PageNavigationHost can forward system Back to the active page first.
        page = this.FindAncestorOfType<Page>();
        if (page is { } containingPage)
        {
            containingPage.PageNavigationSystemBackButtonPressed += OnHostBackRequested;
        }

        if (IsExpanded)
        {
            OpenSheet();
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RemoveFromOpenSheets();
        CancelTransition();
        SheetLayer.IsVisible = false;
        Backdrop.Opacity = 0;

        if (page is { } containingPage)
        {
            containingPage.PageNavigationSystemBackButtonPressed -= OnHostBackRequested;
            page = null;
        }

        if (topLevel is { } host)
        {
            host.RemoveHandler(KeyDownEvent, OnHostKeyDown);
            host.RemoveHandler(PointerPressedEvent, OnHostPointerPressed);
            host.BackRequested -= OnHostBackRequested;
            topLevel = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsExpandedProperty && this.IsAttachedToVisualTree())
        {
            CancelTransition();
            if (IsExpanded)
            {
                OpenSheet();
            }
            else
            {
                CloseSheet();
            }
        }

        if (isInitializingLayout || isRestoringLayout || change.Property != ContentProperty)
        {
            return;
        }

        mainContent = change.GetNewValue<object?>();
        mainContentPresenter!.Content = mainContent;

        isRestoringLayout = true;
        base.Content = layoutContent;
        isRestoringLayout = false;
    }

    private void OpenSheet()
    {
        if (topLevel is { } host)
        {
            OpenSheets.GetOrCreateValue(host).Add(this);
        }

        if (ResetScrollPosition)
        {
            SheetScrollViewer.Offset = default;
        }

        // An outgoing transition can hide the presenter or leave its animated values in place.
        SheetContentPresenter.IsVisible = true;
        SheetContentPresenter.Opacity = 1;
        SheetContentPresenter.RenderTransform = null;
        SheetLayer.IsVisible = true;

        LayoutUpdated += OnOpeningLayoutUpdated;
        SheetLayer.InvalidateArrange();
    }

    private void OnOpeningLayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnOpeningLayoutUpdated;
        Backdrop.Opacity = 1;

        // PageSlide uses the arranged size of the sheet's parent for its travel distance.
        if (OpenTransition is { } transition)
        {
            StartTransition(transition, opening: true);
        }
    }

    private void CloseSheet()
    {
        RemoveFromOpenSheets();
        Backdrop.Opacity = 0;
        if (SheetLayer.IsVisible && CloseTransition is { } transition)
        {
            StartTransition(transition, opening: false);
        }
        else
        {
            FinishClosing();
        }
    }

    private void FinishClosing()
    {
        SheetLayer.IsVisible = false;
        SheetClosed?.Invoke(this, EventArgs.Empty);
    }

    private async void StartTransition(IPageTransition transition, bool opening)
    {
        var forward = opening;
        if (transition is PageSlide slide && ReferenceEquals(slide, Resources["DefaultSheetSlide"]))
        {
            var ticks = Math.Round(slide.Duration.Ticks / TransitionSpeed);
            var duration = ticks >= TimeSpan.MaxValue.Ticks
                ? TimeSpan.MaxValue
                : TimeSpan.FromTicks(Math.Max(1, (long)ticks));

            // Style resources can be reused. Give this playback its own duration.
            var axis = Placement is Dock.Left or Dock.Right
                ? PageSlide.SlideAxis.Horizontal
                : PageSlide.SlideAxis.Vertical;
            // A forward slide enters from the positive side and exits toward the negative side.
            forward = opening == (Placement is Dock.Bottom or Dock.Right);
            transition = new PageSlide(duration, axis)
            {
                FillMode = slide.FillMode,
                SlideInEasing = slide.SlideInEasing,
                SlideOutEasing = slide.SlideOutEasing,
            };
        }

        using var cancellation = new CancellationTokenSource();
        activeTransition = cancellation;
        try
        {
            await transition.Start(
                opening ? null : SheetContentPresenter,
                opening ? SheetContentPresenter : null,
                forward,
                cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            if (activeTransition == cancellation)
            {
                activeTransition = null;
                if (!opening)
                {
                    FinishClosing();
                }
            }
        }
    }

    private void CancelTransition()
    {
        LayoutUpdated -= OnOpeningLayoutUpdated;
        var cancellation = activeTransition;
        activeTransition = null;
        cancellation?.Cancel();
    }

    private void OnHostKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Escape && CanHandleDismissal() && !IsModal)
        {
            Dismiss(e);
        }
    }

    private void OnHostBackRequested(object? sender, RoutedEventArgs e)
    {
        if (e.Handled || !CanHandleDismissal())
        {
            return;
        }

        if (IsModal)
        {
            // Keep system Back from navigating away while a modal sheet is open.
            e.Handled = true;
            return;
        }

        Dismiss(e);
    }

    private void OnHostPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Handled && CanHandleDismissal() && !IsModal &&
            (e.Pointer.Type == PointerType.Touch || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) &&
            e.Source is Visual source && !IsInsideSheet(source))
        {
            Dismiss(e);
        }
    }

    private bool CanHandleDismissal()
    {
        return IsExpanded && IsEffectivelyVisible &&
            topLevel is { } host && OpenSheets.TryGetValue(host, out var sheets) &&
            ReferenceEquals(sheets.FindLast(sheet => sheet.IsEffectivelyVisible), this);
    }

    private void RemoveFromOpenSheets()
    {
        if (topLevel is { } host && OpenSheets.TryGetValue(host, out var sheets))
        {
            sheets.Remove(this);
        }
    }

    private bool IsInsideSheet(Visual source)
    {
        if (source == SheetContentPresenter || SheetContentPresenter.IsVisualAncestorOf(source))
        {
            return true;
        }

        // Overlay popups are rendered outside the sheet's visual subtree.
        return source.FindAncestorOfType<OverlayPopupHost>(includeSelf: true)?.Parent is Popup popup &&
            SheetContentPresenter.IsVisualAncestorOf(popup.PlacementTarget ?? popup);
    }

    private void Dismiss(RoutedEventArgs e)
    {
        e.Handled = true;
        SetCurrentValue(IsExpandedProperty, false);
    }
}
