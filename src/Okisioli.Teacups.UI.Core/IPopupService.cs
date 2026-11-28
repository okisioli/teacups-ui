namespace Okisioli.Teacups.UI.Core;

/// <summary>
/// Shows transient, non-modal feedback within the application.
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// Briefly displays a message that requires no user interaction.
    /// </summary>
    /// <param name="text">The message to display.</param>
    void ShowToast(string text);

    /// <summary>
    /// Displays a message with an Undo button.
    /// </summary>
    /// <param name="text">The message to display.</param>
    /// <param name="undoAction">The action to invoke when the user selects Undo.</param>
    void ShowSnackbar(string text, Action undoAction);
}
