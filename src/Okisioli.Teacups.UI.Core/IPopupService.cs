namespace Okisioli.Teacups.UI.Core;

/// <summary>
/// Shows transient, non-modal feedback within the application.
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// Queues a brief, passive message for display.
    /// </summary>
    /// <param name="text">The message to display.</param>
    void ShowToast(string text);

    /// <summary>
    /// Displays a timed message with an action button, replacing any active snackbar.
    /// </summary>
    /// <param name="text">The message to display.</param>
    /// <param name="actionText">The label of the action button.</param>
    /// <param name="action">The callback invoked when the user selects the action.</param>
    void ShowSnackbar(string text, string actionText, Action action);
}
