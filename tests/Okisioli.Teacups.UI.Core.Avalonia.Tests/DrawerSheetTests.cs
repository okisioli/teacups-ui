using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class DrawerSheetTests(AvaloniaUiThread uiThread)
{
    private static readonly Size OverlaySize = new(320, 240);

    [Fact]
    public Task SheetAlignment_DefaultValues_FillsOverlay() => uiThread.RunAsync(() =>
    {
        var drawer = new DrawerSheet { SheetContent = new Border { Width = 40, Height = 30 } };

        WithOpenDrawer(drawer, _ =>
            Assert.Equal(new Rect(OverlaySize), drawer.FindControl<ContentPresenter>("SheetContentPresenter")!.Bounds));
        return Task.CompletedTask;
    });

    [Fact]
    public Task SheetAlignment_RightAndBottom_UsesMinimumSize() => uiThread.RunAsync(() =>
    {
        var drawer = new DrawerSheet
        {
            SheetContent = new Border { Width = 40, Height = 30 },
            SheetHorizontalAlignment = HorizontalAlignment.Right,
            SheetVerticalAlignment = VerticalAlignment.Bottom,
            SheetMinWidth = 120,
            SheetMinHeight = 90
        };

        WithOpenDrawer(drawer, _ =>
            Assert.Equal(new Rect(200, 150, 120, 90), drawer.FindControl<ContentPresenter>("SheetContentPresenter")!.Bounds));
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(Dock.Bottom)]
    [InlineData(Dock.Left)]
    [InlineData(Dock.Top)]
    [InlineData(Dock.Right)]
    public Task SheetContent_ExceedsOverlay_CanScrollToEnd(Dock placement) => uiThread.RunAsync(() =>
    {
        var drawer = new DrawerSheet
        {
            Placement = placement,
            SheetContent = new Border { Width = 640, Height = 480 },
            SheetHorizontalAlignment = HorizontalAlignment.Center,
            SheetVerticalAlignment = VerticalAlignment.Center
        };

        WithOpenDrawer(drawer, _ => ScrollToEnd(drawer.FindControl<ScrollViewer>("SheetScrollViewer")!));
        return Task.CompletedTask;
    });

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public Task IsExpanded_ReopenedAfterScrolling_UsesResetSetting(bool? resetScrollPosition) => uiThread.RunAsync(() =>
    {
        var drawer = new DrawerSheet { SheetContent = new Border { Width = 640, Height = 480 } };
        if (resetScrollPosition is { } reset)
        {
            drawer.ResetScrollPosition = reset;
        }

        WithOpenDrawer(drawer, window =>
        {
            var scroller = drawer.FindControl<ScrollViewer>("SheetScrollViewer")!;
            var initialOffset = scroller.Offset;
            Assert.Equal(default, initialOffset);
            var end = ScrollToEnd(scroller);
            window.UpdateLayout();

            drawer.IsExpanded = false;
            window.UpdateLayout();
            drawer.IsExpanded = true;
            window.UpdateLayout();

            Assert.Equal(resetScrollPosition == false ? end : initialOffset, scroller.Offset);
        });
        return Task.CompletedTask;
    });

    private static Vector ScrollToEnd(ScrollViewer scroller)
    {
        Assert.True(scroller.Extent.Width > scroller.Viewport.Width);
        Assert.True(scroller.Extent.Height > scroller.Viewport.Height);
        var end = new Vector(scroller.Extent.Width - scroller.Viewport.Width,
            scroller.Extent.Height - scroller.Viewport.Height);
        scroller.Offset = end;
        Assert.Equal(end, scroller.Offset);
        return end;
    }

    private static void WithOpenDrawer(DrawerSheet drawer, Action<Window> check)
    {
        drawer.OpenTransition = null;
        drawer.CloseTransition = null;
        drawer.IsExpanded = true;
        var window = new Window
        {
            Width = OverlaySize.Width,
            Height = OverlaySize.Height,
            Content = drawer,
            ShowInTaskbar = false,
            ShowActivated = false,
            Opacity = 0
        };
        try
        {
            window.Show();
            check(window);
        }
        finally
        {
            window.Close();
        }
    }
}
