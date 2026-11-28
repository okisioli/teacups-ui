using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
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

    private static Grid CreateDemoView()
    {
        var navigationPage = new NavigationPage();
        var dialogService = new DialogService<DialogViewModel, DialogView>(navigationPage, confirmOnBackNavigation: false);
        var notificationManager = new WindowNotificationManager { Position = NotificationPosition.BottomCenter };
        var popupService = new PopupService(notificationManager);
        var demoPage = new ContentPage { Content = new MainWindow(dialogService, popupService) };
        NavigationPage.SetHasNavigationBar(demoPage, false);
        navigationPage.Content = demoPage;
        return new Grid { Children = { navigationPage, notificationManager } };
    }
}
