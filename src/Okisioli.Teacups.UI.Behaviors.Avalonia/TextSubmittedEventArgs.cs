namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

/// <summary>Contains the text captured at submission, before optional clearing.</summary>
public sealed class TextSubmittedEventArgs(string text) : EventArgs
{
    /// <summary>Gets the exact text captured at submission, including an empty string.</summary>
    public string Text { get; } = text;
}
