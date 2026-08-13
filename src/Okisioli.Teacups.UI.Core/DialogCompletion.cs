namespace Okisioli.Teacups.UI.Core;

/// <summary>
/// Represents the completion state of one dialog instance.
/// </summary>
public sealed class DialogCompletion : IDialogCompletion
{
    private readonly TaskCompletionSource<bool> result = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets the result of the dialog.
    /// </summary>
    public Task<bool> Result => result.Task;

    /// <inheritdoc />
    public void Confirm()
    {
        result.TrySetResult(true);
    }

    /// <inheritdoc />
    public void Cancel()
    {
        result.TrySetResult(false);
    }
}
