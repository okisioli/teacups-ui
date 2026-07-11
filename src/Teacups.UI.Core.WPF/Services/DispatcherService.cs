using System.Windows.Threading;
using Teacups.UI.Core.Services;
using Priority = Teacups.UI.Core.Services.IDispatcherService.DispatcherPriority;

namespace Teacups.UI.Core.WPF.Services;

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