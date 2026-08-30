namespace Okisioli.Teacups.UI.Core;

/// <summary>Schedules work on the UI dispatcher.</summary>
public interface IDispatcherService
{
    /// <summary>Invokes an action on the UI dispatcher.</summary>
    /// <param name="action">The action to invoke.</param>
    /// <param name="priority">The priority for the dispatched action.</param>
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    /// <summary>Defines the priority for a dispatched action.</summary>
    public enum DispatcherPriority
    {
        /// <summary>Schedules normal UI work.</summary>
        Normal,
        /// <summary>Schedules high-priority UI work.</summary>
        High,
    }
}
