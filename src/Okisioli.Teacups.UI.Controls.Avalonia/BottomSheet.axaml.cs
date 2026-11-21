using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    private object? mainContent;
    private object? layoutContent;
    private ContentPresenter? mainContentPresenter;
    private TopLevel? topLevel;
    private Page? page;
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
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
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
