namespace Okisioli.Teacups.UI.Core;

public interface IDispatcherService
{
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    public enum DispatcherPriority
    {
        Normal,
        High,
    }
}
