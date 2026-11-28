using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Okisioli.Teacups.UI.Controls.Avalonia;
using Okisioli.Teacups.UI.Core.Avalonia;

namespace Okisioli.Teacups.UI.Demo.Avalonia;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Window
            {
                Content = CreateDemoView(),
                Width = 760,
                Height = 500,
                MinWidth = 620,
                MinHeight = 420,
                Title = "Teacups UI demo"
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = CreateDemoView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static RootView CreateDemoView()
    {
        var navigationPage = new NavigationPage();
        var dialogSheet = new DrawerSheet { Content = navigationPage };
        var dialogService = new DialogService<DialogViewModel, DialogView>(navigationPage, dialogSheet, confirmOnBackNavigation: false);
        var root = new RootView { MainContent = dialogSheet };
        var popupService = new PopupService(root);
        var demoPage = new ContentPage { Content = new MainWindow(dialogService, popupService) };
        NavigationPage.SetHasNavigationBar(demoPage, false);
        navigationPage.Content = demoPage;
        return root;
    }
}
