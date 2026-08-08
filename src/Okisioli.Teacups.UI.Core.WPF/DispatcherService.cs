using System.Windows.Threading;
using Okisioli.Teacups.UI.Core;
using Priority = Okisioli.Teacups.UI.Core.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Core.WPF;

public class DispatcherService(Dispatcher dispatcher) : IDispatcherService
{
    public Task InvokeAsync(Action action, Priority priority = Priority.Normal)
    {
        if (dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, priority == Priority.Normal
                    ? DispatcherPriority.ApplicationIdle
                    : DispatcherPriority.Input)
            .Task;
    }
}
