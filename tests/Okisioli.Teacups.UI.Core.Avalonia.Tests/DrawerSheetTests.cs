using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection<AvaloniaUiTests>]
public sealed class DrawerSheetTests(AvaloniaUiThread uiThread)
{
    private static readonly Size OverlaySize = new(320, 240);

    [Fact]
    public Task SheetAlignment_DefaultValues_FillsOverlay() => uiThread.RunAsync(() =>
    {
        var drawer = new DrawerSheet { SheetContent = new Border { Width = 40, Height = 30 } };

        ArrangeOverlay(drawer);

        Assert.Equal(new Rect(OverlaySize), drawer.FindControl<ContentPresenter>("SheetContentPresenter")!.Bounds);
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

        ArrangeOverlay(drawer);

        Assert.Equal(new Rect(200, 150, 120, 90), drawer.FindControl<ContentPresenter>("SheetContentPresenter")!.Bounds);
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

        ArrangeOverlay(drawer);

        var scroller = drawer.FindControl<ScrollViewer>("SheetScrollViewer")!;
        Assert.True(scroller.Extent.Width > scroller.Viewport.Width);
        Assert.True(scroller.Extent.Height > scroller.Viewport.Height);
        var end = new Vector(scroller.Extent.Width - scroller.Viewport.Width,
            scroller.Extent.Height - scroller.Viewport.Height);
        scroller.Offset = end;
        Assert.Equal(end, scroller.Offset);
        return Task.CompletedTask;
    });

    private static void ArrangeOverlay(DrawerSheet drawer)
    {
        var layer = drawer.FindControl<Grid>("SheetLayer")!;
        layer.IsVisible = true;
        drawer.FindControl<ContentPresenter>("SheetContentPresenter")!.UpdateChild();
        layer.Measure(OverlaySize);
        layer.Arrange(new Rect(OverlaySize));
    }
}
