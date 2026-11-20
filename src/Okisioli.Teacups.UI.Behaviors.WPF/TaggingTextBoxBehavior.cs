using System.Windows;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Submits the current text on Enter or when the user enters a delimiter.</summary>
/// <remarks>
/// The entire text is submitted as entered, including empty strings and surrounding whitespace.
/// Use <see cref="System.Windows.Input.ICommand.CanExecute"/> on the submission command to validate it.
/// </remarks>
public sealed class TaggingTextBoxBehavior : TextBoxSubmitBehavior
{
    public static readonly DependencyProperty DelimiterProperty =
        DependencyProperty.Register(nameof(Delimiter), typeof(char), typeof(TaggingTextBoxBehavior), new PropertyMetadata(','));

    static TaggingTextBoxBehavior()
    {
        ClearOnSubmitProperty.OverrideMetadata(typeof(TaggingTextBoxBehavior), new PropertyMetadata(true));
    }

    /// <summary>Gets or sets the single delimiter character. Defaults to a comma.</summary>
    /// <remarks>
    /// Use <c>Delimiter="&amp;#32;"</c> in XAML for a space. Matching uses committed text input,
    /// so it follows the keyboard layout and IME character entry. A standalone delimiter is
    /// consumed. Multi-character input and pasted text are not split into tags.
    /// </remarks>
    public char Delimiter
    {
        get => (char)GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }

    protected override bool IsSubmitText(string? text) =>
        text is { Length: 1 } && text[0] == Delimiter;
}
