using System.Windows.Threading;
using Okisioli.Teacups.UI.Core;
using Priority = Okisioli.Teacups.UI.Core.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Core.WPF;

/// <summary>Schedules work on the WPF UI dispatcher.</summary>
/// <param name="dispatcher">The WPF dispatcher to use.</param>
public class DispatcherService(Dispatcher dispatcher) : IDispatcherService
{
    /// <inheritdoc />
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
