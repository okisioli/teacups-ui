using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Okisioli.Teacups.UI.Core.Avalonia;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var navigationPage = new NavigationPage();
            var demoPage = new ContentPage
            {
                Content = new MainWindow(new DialogService<DialogViewModel, DialogView>(navigationPage, confirmOnBackNavigation: false))
            };
            NavigationPage.SetHasNavigationBar(demoPage, false);
            navigationPage.Content = demoPage;

            desktop.MainWindow = new Window
            {
                Content = navigationPage,
                Width = 760,
                Height = 500,
                MinWidth = 620,
                MinHeight = 420,
                Title = "Teacups UI demo"
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
