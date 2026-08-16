using Okisioli.Teacups.UI.Core;

namespace Okisioli.Teacups.UI.Demo.WPF;

public sealed class DialogViewModel(IDialogCompletion completion)
{
    public void Confirm() => completion.Confirm();

    public void Cancel() => completion.Cancel();
}
