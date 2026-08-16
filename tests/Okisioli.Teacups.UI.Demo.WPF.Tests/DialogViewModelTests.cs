using Okisioli.Teacups.UI.Core;
using Okisioli.Teacups.UI.Demo.WPF;
using Xunit;

namespace Okisioli.Teacups.UI.Demo.WPF.Tests;

public sealed class DialogViewModelTests
{
    [Fact]
    public void Confirm_ConfirmsDialog()
    {
        var completion = new TestDialogCompletion();

        new DialogViewModel(completion).Confirm();

        Assert.True(completion.IsConfirmed);
        Assert.False(completion.IsCancelled);
    }

    [Fact]
    public void Cancel_CancelsDialog()
    {
        var completion = new TestDialogCompletion();

        new DialogViewModel(completion).Cancel();

        Assert.False(completion.IsConfirmed);
        Assert.True(completion.IsCancelled);
    }

    private sealed class TestDialogCompletion : IDialogCompletion
    {
        public bool IsConfirmed { get; private set; }

        public bool IsCancelled { get; private set; }

        public void Confirm() => IsConfirmed = true;

        public void Cancel() => IsCancelled = true;
    }
}
