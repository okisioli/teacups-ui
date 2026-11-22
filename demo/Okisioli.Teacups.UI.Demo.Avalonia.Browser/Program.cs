using Avalonia;
using Avalonia.Browser;

namespace Okisioli.Teacups.UI.Demo.Avalonia.Browser;

internal static class Program
{
    private static Task Main()
    {
        return AppBuilder.Configure<App>()
            .LogToTrace()
            .StartBrowserAppAsync("demo");
    }
}
