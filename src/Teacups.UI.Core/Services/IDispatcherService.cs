namespace Teacups.UI.Core.Services;

public interface IDispatcherService
{
    Task InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal);

    public enum DispatcherPriority
    {
        Normal,
        High,
    }
}