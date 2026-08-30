using Avalonia.Threading;
using Okisioli.Teacups.UI.Core;
using Priority = Okisioli.Teacups.UI.Core.IDispatcherService.DispatcherPriority;

namespace Okisioli.Teacups.UI.Core.Avalonia;

/// <summary>Schedules work on the Avalonia UI dispatcher.</summary>
/// <param name="dispatcher">The Avalonia dispatcher to use.</param>
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

        return dispatcher.InvokeAsync(async () => action(), priority == Priority.Normal
                    ? DispatcherPriority.ApplicationIdle
                    : DispatcherPriority.Input);
    }
}
