namespace Okisioli.Teacups.UI.Core;

public interface IDispatcherService
{
    /// <summary>
    /// Queues an action on the UI dispatcher, including when called from the UI thread.
    /// </summary>
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    public enum DispatcherPriority
    {
        // Preserve Normal and the former High numeric values for compiled callers.
        Normal = 0,
        Input = 1,
        Background = 2,
        ApplicationIdle = 3,
    }
}
