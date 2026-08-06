using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Okisioli.Teacups.Controls.Avalonia.Demo;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
