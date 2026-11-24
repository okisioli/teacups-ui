using Avalonia.Controls;
using Xunit;

namespace Okisioli.Teacups.UI.Core.Avalonia.Tests;

[Collection(nameof(AvaloniaUiTests))]
public sealed class DialogServiceTests(AvaloniaUiThread uiThread)
{
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
        var window = new Window { Content = navigation, ShowInTaskbar = false };
        try
        {
            window.Show();
            await navigation.PushAsync(previousPage).WaitAsync(AvaloniaUiThread.Timeout);

            var service = new DialogService<IDialogCompletion, UserControl>(navigation, confirmOnBackNavigation);
            var result = service.ShowDialogAsync(completion => completion);

            var page = Assert.IsType<ContentPage>(navigation.CurrentPage);
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
