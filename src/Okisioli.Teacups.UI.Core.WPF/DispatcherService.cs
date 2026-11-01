using System.Windows.Threading;
using Okisioli.Teacups.UI.Core;
using Priority = Okisioli.Teacups.UI.Core.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Core.WPF;

public class DispatcherService(Dispatcher dispatcher) : IDispatcherService
{
    public Task InvokeAsync(Action action, Priority priority = Priority.Normal)
    {
        var dispatcherPriority = priority switch
        {
            Priority.ApplicationIdle => DispatcherPriority.ApplicationIdle,
            Priority.Background => DispatcherPriority.Background,
            Priority.Input => DispatcherPriority.Input,
            Priority.Normal => DispatcherPriority.Normal,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };

        return dispatcher.InvokeAsync(action, dispatcherPriority).Task;
    }
}
