namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

/// <summary>Contains the text captured at submission, before optional clearing.</summary>
public sealed class TextSubmittedEventArgs(string text) : EventArgs
{
    public string Text { get; } = text;
}
