namespace Okisioli.Teacups.UI.Core;

public interface IDispatcherService
{
    /// <summary>
    /// Queues an action on the UI dispatcher, including when called from the UI thread.
    /// </summary>
    /// <param name="action">The action to run on the UI thread.</param>
    /// <param name="priority">The queue priority. Defaults to <see cref="DispatcherPriority.Normal"/>.</param>
    /// <returns>A task that completes when the queued action finishes.</returns>
    /// <remarks>
    /// Use <see cref="DispatcherPriority.Background"/> for incremental collection updates
    /// that should yield to input and rendering.
    /// </remarks>
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    /// <summary>Specifies UI dispatcher queue priorities.</summary>
    /// <remarks>
    /// From lowest to highest, the priorities are <see cref="ApplicationIdle"/>,
    /// <see cref="Background"/>, <see cref="Input"/>, and <see cref="Normal"/>.
    /// WPF and Avalonia use the corresponding native priorities.
    /// Replace references to the former <c>High</c> priority with <see cref="Input"/>.
    /// </remarks>
    public enum DispatcherPriority
    {
        // Preserve Normal and the former High numeric values for compiled callers.
        Normal = 0,
        Input = 1,
        Background = 2,
        ApplicationIdle = 3,
    }
}
