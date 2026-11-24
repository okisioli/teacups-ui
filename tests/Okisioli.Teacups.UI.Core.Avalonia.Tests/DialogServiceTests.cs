using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class DialogServiceTests(AvaloniaUiThread uiThread)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ShowDialogAsync_DefaultPresentation_ShowsCenteredSheetAndReturnsResult(bool confirm)
    {
        return uiThread.RunAsync(async () =>
        {
            var page = new ContentPage();
            var navigation = new NavigationPage { Content = page };
            var sheet = new DrawerSheet { Content = navigation, CloseTransition = null };
            var window = new Window { Content = sheet, ShowInTaskbar = false };
            try
            {
                window.Show();
                var service = new DialogService<IDialogCompletion, UserControl>(navigation, sheet, confirmOnBackNavigation: true);
                var result = service.ShowDialogAsync(completion => completion);

                Assert.True(sheet.IsExpanded);
                Assert.Equal(HorizontalAlignment.Center, sheet.SheetHorizontalAlignment);
                Assert.Equal(VerticalAlignment.Center, sheet.SheetVerticalAlignment);
                Assert.Same(page, navigation.CurrentPage);
                var view = Assert.IsType<UserControl>(sheet.SheetContent);
                if (confirm)
                {
                    Assert.IsAssignableFrom<IDialogCompletion>(view.DataContext).Confirm();
                }
                else
                {
                    window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                }

                Assert.Equal(confirm, await result.WaitAsync(AvaloniaUiThread.Timeout));
                Assert.False(sheet.IsExpanded);
                Assert.Null(sheet.SheetContent);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Escape_NestedSheetsOpen_DismissesLastOpenedFirst(bool outerOpenedLast)
    {
        return uiThread.RunAsync(() =>
        {
            var inner = new DrawerSheet { SheetContent = "Inner", CloseTransition = null };
            var outer = new DrawerSheet { Content = inner, SheetContent = "Outer", CloseTransition = null };
            var window = new Window { Content = outer, ShowInTaskbar = false };
            try
            {
                window.Show();
                var first = outerOpenedLast ? inner : outer;
                var second = outerOpenedLast ? outer : inner;
                first.IsExpanded = true;
                second.IsExpanded = true;

                window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                Assert.True(first.IsExpanded);
                Assert.False(second.IsExpanded);

                window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                Assert.False(first.IsExpanded);
            }
            finally
            {
                window.Close();
            }

            return Task.CompletedTask;
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ShowDialogAsync_CompletedFromView_RestoresPreviousPageAndReturnsResult(bool confirm)
    {
        return uiThread.RunAsync(() => CheckDialogAsync(
            confirmOnBackNavigation: !confirm,
            expectedResult: confirm,
            (_, completion) =>
            {
                if (confirm)
                {
                    completion.Confirm();
                }
                else
                {
                    completion.Cancel();
                }

                return Task.CompletedTask;
            }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task ShowDialogAsync_BackNavigation_RestoresPreviousPageAndReturnsConfiguredResult(bool confirm)
    {
        return uiThread.RunAsync(() => CheckDialogAsync(confirm, confirm, async (navigation, _) =>
        {
            var dialogPage = navigation.CurrentPage;
            Assert.Same(dialogPage, await navigation.PopAsync());
        }));
    }

    private static async Task CheckDialogAsync(
        bool confirmOnBackNavigation, bool expectedResult,
        Func<NavigationPage, IDialogCompletion, Task> dismiss)
    {
        var previousPage = new ContentPage();
        var navigation = new NavigationPage { PageTransition = null };
        var sheet = new DrawerSheet { Content = navigation };
        var window = new Window { Content = sheet, ShowInTaskbar = false };
        try
        {
            window.Show();
            await navigation.PushAsync(previousPage).WaitAsync(AvaloniaUiThread.Timeout);

            var service = new DialogService<IDialogCompletion, UserControl>(navigation, sheet, confirmOnBackNavigation)
            {
                FullScreen = true
            };
            var result = service.ShowDialogAsync(completion => completion);

            var page = Assert.IsType<ContentPage>(navigation.CurrentPage);
            Assert.False(sheet.IsExpanded);
            Assert.NotSame(previousPage, page);
            var view = Assert.IsType<UserControl>(page.Content);
            var completion = Assert.IsAssignableFrom<IDialogCompletion>(view.DataContext);
            Assert.False(result.IsCompleted);

            await dismiss(navigation, completion).WaitAsync(AvaloniaUiThread.Timeout);
            Assert.Equal(expectedResult, await result.WaitAsync(AvaloniaUiThread.Timeout));
            Assert.Same(previousPage, navigation.CurrentPage);
            Assert.Same(previousPage, Assert.Single(navigation.NavigationStack));
        }
        finally
        {
            window.Close();
        }
    }
}
