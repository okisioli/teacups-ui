namespace Okisioli.Teacups.UI.Core;

/// <summary>
/// Provides a platform-specific way to show a dialog view for a view model.
/// </summary>
/// <typeparam name="TViewModel">The type of the dialog view model.</typeparam>
/// <typeparam name="TView">The type of the dialog view.</typeparam>
public interface IDialogService<TViewModel, TView>
{
    /// <summary>
    /// Shows the dialog for the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to display in the dialog.</param>
    /// <returns><see langword="true"/> when the dialog is accepted; otherwise, <see langword="false"/>.</returns>
    Task<bool> ShowDialogAsync(TViewModel viewModel);

    /// <summary>
    /// Creates and shows a dialog view model with a completion mediator scoped to that dialog instance.
    /// </summary>
    /// <param name="viewModelFactory">Creates the view model that will use the supplied completion mediator.</param>
    /// <returns><see langword="true"/> when the dialog is accepted; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Use this overload when the view model needs to control the result.
    /// Call <see cref="IDialogCompletion.Confirm"/> or <see cref="IDialogCompletion.Cancel"/>
    /// from the view model's commands using the completion object passed to the factory.
    /// </remarks>
    Task<bool> ShowDialogAsync(Func<IDialogCompletion, TViewModel> viewModelFactory);
}
