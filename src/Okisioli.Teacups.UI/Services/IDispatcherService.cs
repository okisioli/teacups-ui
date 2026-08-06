namespace Okisioli.Teacups.UI.Services;

public interface IDispatcherService
{
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    public enum DispatcherPriority
    {
        Normal,
        High,
    }
}
