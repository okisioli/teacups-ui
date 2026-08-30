using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Metadata;

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

    private object? mainContent;
    private object? layoutContent;
    private ContentPresenter? mainContentPresenter;
    private bool isInitializingLayout;
    private bool isRestoringLayout;

    /// <summary>Initializes a bottom-sheet control.</summary>
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
}
