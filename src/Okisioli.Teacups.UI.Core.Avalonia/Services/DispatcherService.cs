using Avalonia.Threading;
using Okisioli.Teacups.UI.Services;
using Priority = Okisioli.Teacups.UI.Services.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Core.Avalonia.Services;

public class DispatcherService(Dispatcher dispatcher) : IDispatcherService
{
    public Task InvokeAsync(Action action, Priority priority = Priority.Normal)
    {
        if (dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(async () => action(), priority == Priority.Normal
                    ? DispatcherPriority.ApplicationIdle
                    : DispatcherPriority.Input);
    }
}
