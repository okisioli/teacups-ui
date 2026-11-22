using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// Displays content with an optional sheet overlaid at its bottom edge.
/// </summary>
public partial class BottomSheet : UserControl
{
    /// <summary>
    /// Defines the content displayed in the bottom sheet.
    /// </summary>
    public static readonly StyledProperty<object?> SheetContentProperty =
        AvaloniaProperty.Register<BottomSheet, object?>(nameof(SheetContent));

    /// <summary>
    /// Defines whether the bottom sheet is visible.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<BottomSheet, bool>(nameof(IsExpanded), defaultValue: false);

    /// <summary>
    /// Defines whether the sheet stays open when Escape, system Back, or an outside press occurs.
    /// </summary>
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<BottomSheet, bool>(nameof(IsModal), defaultValue: false);

    /// <summary>
    /// Defines the transition played when the sheet opens.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> OpenTransitionProperty =
        AvaloniaProperty.Register<BottomSheet, IPageTransition?>(nameof(OpenTransition));

    /// <summary>
    /// Defines the brush covering the main content when the sheet is open.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackdropBrushProperty =
        AvaloniaProperty.Register<BottomSheet, IBrush?>(nameof(BackdropBrush));

    private object? mainContent;
    private object? layoutContent;
    private ContentPresenter? mainContentPresenter;
    private TopLevel? topLevel;
    private Page? page;
    private CancellationTokenSource? openingTransition;
    private bool isInitializingLayout;
    private bool isRestoringLayout;

    public BottomSheet()
    {
        isInitializingLayout = true;
        InitializeComponent();
        isInitializingLayout = false;

        layoutContent = base.Content;
        mainContentPresenter = this.FindControl<ContentPresenter>("MainContentPresenter");
    }

    /// <summary>
    /// Gets or sets the content displayed behind the bottom sheet.
    /// </summary>
    [Content]
    public new object? Content
    {
        get => mainContent;
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed in the bottom sheet.
    /// </summary>
    public object? SheetContent
    {
        get => GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the bottom sheet is visible.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
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
    /// Gets or sets the opening transition. Defaults to a vertical slide. Set to
    /// <see langword="null"/> to show the sheet immediately.
    /// </summary>
    /// <remarks>A custom transition receives the sheet as the incoming visual.</remarks>
    public IPageTransition? OpenTransition
    {
        get => GetValue(OpenTransitionProperty);
        set => SetValue(OpenTransitionProperty, value);
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
            LayoutUpdated += OnOpeningLayoutUpdated;
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelOpening();

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
            if (IsExpanded)
            {
                LayoutUpdated += OnOpeningLayoutUpdated;
            }
            else
            {
                CancelOpening();
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

    private async void OnOpeningLayoutUpdated(object? sender, EventArgs e)
    {
        LayoutUpdated -= OnOpeningLayoutUpdated;
        Backdrop.Opacity = 1;

        // PageSlide uses the arranged height of the sheet's parent for its travel distance.
        if (OpenTransition is { } transition)
        {
            using var cancellation = new CancellationTokenSource();
            openingTransition = cancellation;
            try
            {
                await transition.Start(null, SheetContentPresenter, forward: true, cancellation.Token);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            finally
            {
                if (openingTransition == cancellation)
                {
                    openingTransition = null;
                }
            }
        }
    }

    private void CancelOpening()
    {
        LayoutUpdated -= OnOpeningLayoutUpdated;
        var cancellation = openingTransition;
        openingTransition = null;
        cancellation?.Cancel();
        Backdrop.Opacity = 0;
    }

    private void OnHostKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsExpanded && IsEffectivelyVisible && !IsModal)
        {
            Dismiss(e);
        }
    }

    private void OnHostBackRequested(object? sender, RoutedEventArgs e)
    {
        if (!IsExpanded || !IsEffectivelyVisible)
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
        if (IsExpanded && IsEffectivelyVisible && !IsModal &&
            (e.Pointer.Type == PointerType.Touch || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) &&
            e.Source is Visual source && !IsInsideSheet(source))
        {
            Dismiss(e);
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
