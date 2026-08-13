namespace Okisioli.Teacups.UI.Core;

/// <summary>
/// Completes a dialog that is currently being shown by an <see cref="IDialogService{TViewModel,TView}"/>.
/// </summary>
public interface IDialogCompletion
{
    /// <summary>
    /// Accepts the dialog.
    /// </summary>
    void Confirm();

    /// <summary>
    /// Cancels the dialog.
    /// </summary>
    void Cancel();
}
